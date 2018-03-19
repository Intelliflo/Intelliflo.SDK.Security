/**
 * JenkinsFile for the pipeline of Intelliflo.Platform projects
 * ~~ MANAGED BY DEVOPS ~~
 */

/**
 * By default the master branch of the library is loaded
 * Use the include directive below ONLY if you need to load a branch of the library
 * @Library('intellifloworkflow@IP-34341')
 */

import org.intelliflo.*

def changeset = new Changeset()
def amazon = new Amazon()

def artifactoryCredentialsId = 'a3c63f46-4be7-48cc-869b-4239a869cbe8'
def artifactoryUri = 'https://artifactory.intelliflo.io/artifactory'
def gitCredentialsId = '1327a29c-d426-4f3d-b54a-339b5629c041'
def jiraCredentialsId = '32546070-393c-4c45-afcd-8e8f1de1757b'

def stageName
def semanticVersion
def packageVersion
def globals = env
def verboseLogging = false
def nodeLabel = 'devops'
pipeline {

    agent none

    environment {
        githubRepoName = "${env.JOB_NAME.split('/')[1]}"
        solutionName = "${env.JOB_NAME.split('/')[1].replace('Clone.', '')}"
    }

    options {
        timestamps()
        skipDefaultCheckout()
    }

    stages {
        stage('Initialise') {
            agent none
            steps {
                script {
                    stashResourceFiles {
                        targetPath = 'org/intelliflo'
                        masterNode = 'master'
                        stashName = 'ResourceFiles'
                        resourcePath = "@libs/intellifloworkflow/resources"
                    }

                    abortOlderBuilds {
                        logVerbose = verboseLogging
                    }
                }
            }
        }

        stage('Component') {
            agent {
                label nodeLabel
            }
            steps {
                bat 'set'

                script {
                    stageName = 'Component'

                    // Analyse and validate the changeset
                    validateChangeset {
                        repoName = globals.githubRepoName
                        prNumber = globals.CHANGE_ID
                        baseBranch = globals.CHANGE_TARGET
                        branchName = globals.BRANCH_NAME
                        buildNumber = globals.BUILD_NUMBER
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                        abortOnFailure = true
                    }
                    def json = (String)Consul.getStoreValue(ConsulKey.get(globals.githubRepoName, globals.BRANCH_NAME, globals.CHANGE_ID, 'changeset'))
                    changeset = changeset.fromJson(json)

                    // Checkout the code and unstash supporting scripts
                    checkoutCode {
                        delegate.stageName = stageName
                    }

                    // Scripts required by the pipeline
                    unstashResourceFiles {
                        folder = 'pipeline'
                        stashName = 'ResourceFiles'
                    }

                    // Versioning
                    calculateVersion {
                        buildNumber = globals.BUILD_NUMBER
                        delegate.changeset = changeset
                        delegate.stageName = stageName
                        abortOnFailure = true
                    }

                    semanticVersion = Consul.getStoreValue(ConsulKey.get(env.githubRepoName, env.BRANCH_NAME, globals.CHANGE_ID, 'existing.version'))
                    packageVersion = "${semanticVersion}.${env.BUILD_NUMBER}"
                    if (changeset.pullRequest != null) {
                        currentBuild.displayName = "${githubRepoName}.Pr${changeset.prNumber}(${packageVersion})"
                    } else {
                        currentBuild.displayName = "${githubRepoName}(${packageVersion})"
                    }

                    startSonarQubeAnalysis {
                        repoName = globals.githubRepoName
                        solutionName = globals.solutionName
                        version = semanticVersion
                        branchName = changeset.originatingBranch
                        unitTestResults = "UnitTestResults"
                        coverageResults = "OpenCoverResults"
                        inspectCodeResults = "ResharperInspectCodeResults"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                        testRunnerType = 'vstest'
                    }

                    bat """
                        "C:\\Program Files\\dotnet\\dotnet.exe" build -c Release /p:AssemblyVersion=${packageVersion} /p:Version=${packageVersion}
                    """

                    runDependencyCheck {
                        repoName = "${globals.solutionName}"
                        binariesLocation = "src\\${globals.solutionName}\\bin\\Release\\netstandard2.0"
                        delegate.stageName = stageName
                    }

                    echo "[INFO] running unit tests"

                    def unitTestResults = runDotnetTests{
                        title = "Unit Tests"
                        projectToTest = "${pwd()}/test/Intelliflo.SDK.Security.Tests/Intelliflo.SDK.Security.Tests.csproj"
                        unitTestsResultsFilename ="UnitTestResults"
                        withCoverage = true
                        coverageResultsFilename = "OpenCoverResults"
                        coverageInclude = globals.solutionName
                        pathToPdbs = "test/Intelliflo.SDK.Security.Tests/bin/Release/netcoreapp2.0"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    runResharperInspectCode {
                        repoName = globals.githubRepoName
                        solutionName = globals.solutionName
                        resultsFile = "ResharperInspectCodeResults"
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }                    

                    completeSonarQubeAnalysis {
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    analyseTestResults {
                        title = "Unit Tests"
                        testResults = unitTestResults
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    if (changeset.pullRequest != null) {
                        //pack for pull request
                        bat """
                            "C:\\Program Files\\dotnet\\dotnet.exe" pack ^
                            -c Release ^
                            --no-build ^
                            -o ${pwd()}\\dist ^
                            /p:PackageVersion=${packageVersion}
                        """

                        dir('dist') {
                            stash includes: "*.nupkg", name: 'package'
                        }
                    }

                    if (changeset.branch != null) { 
                        //pack for branch
                        bat """
                            "C:\\Program Files\\dotnet\\dotnet.exe" pack ^
                            -c Release ^
                            --no-build ^
                            -o ${pwd()}\\dist ^
                            /p:PackageVersion=${packageVersion}-alpha
                        """

                        findAndDeleteOldPackages {
                            credentialsId = artifactoryCredentialsId
                            packageName = "${changeset.repoName}.${semanticVersion}"
                            latestBuildNumber = globals.BUILD_NUMBER
                            repos = 'nuget-local'
                            url = artifactoryUri
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        def propset = "github.branch.name=${changeset.branchName} git.repo.name=${changeset.repoName} git.master.sha=${changeset.masterSha} jira.ticket=${changeset.jiraTicket}"
                        
                        publishPackages {
                            credentialsId = artifactoryCredentialsId
                            repo = 'nuget-local'
                            version = "${packageVersion}-alpha"
                            include = "${changeset.repoName}.${packageVersion}-alpha.nupkg"
                            uri = artifactoryUri
                            properties = propset
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }
                    }
                }
            }
            post {
                always {
                    script {
                        archive excludes: 'dist/*.zip,dist/*.nupkg,dist/*.md5', includes: 'dist/**/*.*'
                        deleteWorkspace {
                            force = true
                        }
                    }
                }
            }
        }

        stage('Production') {
            agent none
            when {
                expression { env.BRANCH_NAME ==~ /^PR-.*/ }
            }
            steps {
                script {
                    stageName = 'Production'

                    validateMasterSha {
                        repoName = changeset.repoName
                        packageMasterSha = changeset.masterSha
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    validateCodeReviews {
                        repoName = globals.githubRepoName
                        prNumber = globals.CHANGE_ID
                        author = changeset.author
                        failBuild = false
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    validateJiraTicket {
                        delegate.changeset = changeset
                        failBuild = false
                        delegate.stageName = stageName
                        logVerbose = verboseLogging
                    }

                    node(nodeLabel) {
                        mergePullRequest {
                            repoName = changeset.repoName
                            prNumber = changeset.prNumber
                            masterSha = changeset.masterSha
                            sha = changeset.commitSha
                            consulKey = changeset.consulBaseKey
                            credentialsId = gitCredentialsId
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        bat "if not exist dist\\NUL (mkdir dist)"
                        dir('dist') {
                            unstash 'package'
                        }

                        publishPackageToNugetOrg {
                            delegate.packagePath = "dist\\${globals.solutionName}.${packageVersion}.nupkg"
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        updateJiraOnMerge {
                            issueKey = changeset.jiraTicket
                            packageName = changeset.repoName
                            version = packageVersion
                            credentialsId = jiraCredentialsId
                            logVerbose = verboseLogging
                            delegate.stageName = stageName
                        }

                        deleteDir()
                    }

                    tagCommit {
                        repoName = changeset.repoName
                        version = semanticVersion
                        author = changeset.author
                        email = changeset.commitInfo.author.email
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    updateMasterVersion {
                        repoName = changeset.repoName
                        version = semanticVersion
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    cleanupConsul {
                        repoName = changeset.repoName
                        prNumber = changeset.prNumber
                        consulBuildKey = changeset.consulBuildKey
                        logVerbose = verboseLogging
                        delegate.stageName = stageName
                    }

                    deleteGithubBranch {
                        repoName = changeset.repoName
                        branchName = changeset.originatingBranch
                        logVerbose = verboseLogging
                    }
                }
            }
        }
    }
}