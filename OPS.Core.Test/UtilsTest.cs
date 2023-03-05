namespace OPS.Core.Test
{
    public class UtilsTest
    {
        [Theory()]
        [InlineData(10, 2, "10.00")]
        [InlineData(10, 0, "10")]
        [InlineData(10.4, 0, "10")]
        [InlineData(10.5, 0, "11")]
        [InlineData(-198.76, 1, "-198.8")]
        [InlineData(null, 2, "0.00")]
        [InlineData("", 2, "0.00")]
        [InlineData("0.12", 1, "0.1")]
        public void FormataValor(object value, int decimais, string expect)
        {
            var result = Utils.FormataValor(value, decimais);

            Assert.Equal(expect, result);
        }

        [Theory()]
        [InlineData("2023-02-01", "01/02/2023")]
        [InlineData("2023-02-01 23:59:59", "01/02/2023")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("invalid", "")]
        public void FormataData(object value, string expect)
        {
            var result = Utils.FormataData(value);

            Assert.Equal(expect, result);
        }

        [Theory()]
        [InlineData("2023-02-01", "01/02/2023 00:00")]
        [InlineData("2023-02-01 23:59:59", "01/02/2023 23:59")]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("invalid", "")]
        public void FormataDataHora(object value, string expect)
        {
            var result = Utils.FormataDataHora(value);

            Assert.Equal(expect, result);
        }

        [Theory()]
        [InlineData("12345678912345", "12.345.678/9123-45")]
        [InlineData("invalid", "invalid")]
        public void FormatCNPJ(string value, string expect)
        {
            var result = Utils.FormatCNPJ(value);

            Assert.Equal(expect, result);
        }

        [Theory()]
        [InlineData("12345678909", "***.567.890-**")]
        public void FormatCPF(string value, string expect)
        {
            var result = Utils.FormatCPF(value);

            Assert.Equal(expect, result);
        }

        [Theory()]
        [InlineData("567890", "***.567.890-**")]
        [InlineData("invalid", "invalid")]
        public void FormatCPFParcial(string value, string expect)
        {
            var result = Utils.FormatCPFParcial(value);

            Assert.Equal(expect, result);
        }
    }
}