/**
 * JenkinsFile for the pipeline of Reference Package projects
 * ~~ MANAGED BY DEVOPS ~~
 */

//@Library('intellifloworkflow@IP-36352')
import org.intelliflo.*

def pipelineRuntime = new PipelineRuntime()
def artifactoryCredentialsId = 'a3c63f46-4be7-48cc-869b-4239a869cbe8'
def artifactoryUri = 'https://artifactory.intelliflo.io/artifactory'
def gitCredentialsId = '1327a29c-d426-4f3d-b54a-339b5629c041'
def gitCredentialsSSH = 'jenkinsgithub'
def jiraCredentialsId = '32546070-393c-4c45-afcd-8e8f1de1757b'
def globals = env
def windowsNode = 'windows'
def linuxNode = 'linux'

pipeline {

    agent none

    environment {
        githubRepoName = "${env.JOB_NAME.split('/')[1]}"
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
                }
            }
        }

        stage('Component') {
            agent {
                label windowsNode
            }
            steps {
                bat 'set'

                script {
                    pipelineRuntime = preparePipeline {
                        repoName = globals.githubRepoName
                        prNumber = globals.CHANGE_ID
                        baseBranch = globals.CHANGE_TARGET
                        branchName = globals.BRANCH_NAME
                        buildNumber = globals.BUILD_NUMBER
                        abortOnFailure = true
                        configFile = "Jenkinsfile-config.yml"
                    }

                    buildCode {
                        runtime = pipelineRuntime
                    }

                    pipelineRuntime = unitTest {
                        runtime = pipelineRuntime
                    }

                    runResharperInspectCode {
                        runtime = pipelineRuntime
                    }

                    analyseBuild {
                        runtime = pipelineRuntime
                    }

                    createPackages {
                        runtime = pipelineRuntime
                    }

                    vulnerabilityScan {
                        runtime = pipelineRuntime
                    }

                    pipelineRuntime = publishPackages {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }

                        if (pipelineRuntime.config.componentStage == null || pipelineRuntime.config.componentStage.deleteWorkspace) {
                            deleteWorkspace {
                                force = true
                            }
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
                    pipelineRuntime.currentStage = 'Production'

                    abortOlderBuilds {
                        logVerbose = pipelineRuntime.config.logVerbose
                    }

                    validateProductionStage {
                        runtime = pipelineRuntime
                    }

                    node(windowsNode) {

                        mergeChangeset {
                            runtime = pipelineRuntime
                            credentialsId = gitCredentialsId
                        }

                        unstashResourceFiles {
                            folder = 'pipeline'
                            stashName = 'ResourceFiles'
                        }

                        pipelineRuntime = publishPackages {
                            runtime = pipelineRuntime
                            credentialsId = artifactoryCredentialsId
                            uri = artifactoryUri
                        }

                        updateJiraOnMerge {
                            runtime = pipelineRuntime
                            credentialsId = jiraCredentialsId
                        }

                        deleteDir()
                    }

                    vulnerabilityScan {
                        runtime = pipelineRuntime
                    }

                    cleanUpProduction {
                        runtime = pipelineRuntime
                        credentialsId = artifactoryCredentialsId
                        uri = artifactoryUri
                    }
                }
            }
            post {
                always {
                    script {
                        pipelineRuntime = addTimings {
                            runtime = pipelineRuntime
                        }
                    }
                }
            }
        }
    }

    post {
        always {
            script {
                reportBuildStatusToSlack {
                    changeset = pipelineRuntime.changeset
                }
            }
        }
    }
}