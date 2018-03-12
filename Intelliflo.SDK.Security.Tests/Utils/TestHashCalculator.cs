using FluentAssertions;
using Intelliflo.SDK.Security.Utils;
using Xunit;

namespace Intelliflo.SDK.Security.Tests.Utils
{
    public class TestHashCalculator
    {
        private readonly HashCalculator underTest;

        public TestHashCalculator()
        {
            underTest = new HashCalculator();
        }

        [Theory]
        [InlineData("xx", "aa", "wabuae4aeabbac8aygbqadcauwbzahqauqb6afeargbfaeiawgbyag0aoaa5afmatgbhaeyazga5agwayqarahaatwbyafaargb3aewaygbyahiaywa9aa==")]
        [InlineData("value1", "Password1", "bqbqagsaegbxaggaugbbadaaswbjahaaaqbqaewabqa5afgaqgbuafoamaboaemadgbvafuadabdahmaywbzadaaeabeag0aswbpahcadqbyae0aywa9aa==")]
        public void GetStringToSignHash_Should_Return_Expected_Hash(string value, string secret, string expectedHash)
        {
            underTest.GetStringToSignHash(value, secret).Should().Be(expectedHash);
        }


        [Theory]
        [InlineData("xx", "mqa0agmamqbjadyamqbiadcamabjaduazqa5aguangbhaguamqayadqaoabiadiamgazadkaoaazaduazaa4agqanabmageaygbmaguanwbmageaoaayadqanga1agyazqaxadeaywbladcazqa5adyamaayaguayqaxaduamqa=")]
        [InlineData("value1", "nabjadgaoaa5agmamwayadiazga3aguamgawagqangbiadcazqbhadianwazadaangazagmaywayagyazaayadaamwbiadganwa0adeazqa0adaamga3ageazaaxadeaoqa4adkanwa5adyamwbhadcazqawadyanaa5adeanwa=")]
        public void GetCanonicalRequestHash_Should_Return_Expected_Hash(string value, string expectedHash)
        {
            underTest.GetCanonicalRequestHash(value).Should().Be(expectedHash);
        }
    }
}
