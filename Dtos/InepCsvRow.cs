using CsvHelper.Configuration.Attributes;

namespace Api.Dtos
{
    public class InepCsvRow
    {
        [Name("NO_CURSO")]
        public string NoCurso { get; set; }

        [Name("CO_IES")]
        public string CoIes { get; set; }

        [Name("NO_MUNICIPIO")]
        public string Municipio { get; set; }

        [Name("SG_UF")]
        public string Uf { get; set; }

        // --- NOVO ---
        [Name("NOME_IES_COMPLETO")]
        public string NomeIes { get; set; }
    }
}