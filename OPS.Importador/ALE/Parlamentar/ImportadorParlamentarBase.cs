using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;

namespace OPS.Importador.ALE.Parlamentar
{
    public abstract class ImportadorParlamentarBase : IImportadorParlamentar
    {
        protected readonly ILogger<ImportadorParlamentarBase> logger;
        protected readonly IDbConnection connection;
        protected ImportadorParlamentarConfig config;

        public ImportadorParlamentarBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorParlamentarBase>>();
            connection = serviceProvider.GetService<IDbConnection>();
        }

        public void Configure(ImportadorParlamentarConfig config)
        {
            this.config = config;
        }

        public abstract Task Importar();

        public ushort BuscarIdPartido(string partido)
        {
            // Bahia
            // partido = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA").Replace("REPUB", "REPUBLICANOS");
            // São Paulo
            // partido = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA");
            // Ceara
            // partido = partido.Replace("PC do B", "PCdoB").Replace("PATRI", "PATRIOTA").Replace("REPUB", "REPUBLICANOS");
            // Mato Grosso do Sul
            if (partido == "PATRI") partido = "PATRIOTA";
            else if (partido == "REPUB") partido = "REPUBLICANOS";
            else if (partido == "PARTIDO PROGRESSISTA") partido = "PP"; // Progressistas
            else if (partido == "PARTIDO SOLIDARIEDADE") partido = "SD"; // Solidariedade
            else partido = partido.Replace("PC do B", "PCdoB").Replace("Podemos", "PODE");
            // Minas Gerais
            // partido = partido.Replace("PATRI", "PATRIOTA");

            var IdPartido = connection.GetList<Partido>(new { Sigla = partido }).FirstOrDefault()?.Id;
            if (IdPartido == null)
            {
                IdPartido = connection.GetList<Partido>(new { Nome = partido }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                    throw new Exception($"Partido '{partido}' Inexistenete");
            }

            return IdPartido.Value;
        }

        protected DeputadoEstadual GetDeputadoByNameOrNew(string nomeParlamentar)
        {
            return connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    nome_parlamentar = nomeParlamentar
                })
                .FirstOrDefault()
                ?? new DeputadoEstadual()
                {
                    IdEstado = (ushort)config.Estado,
                    NomeParlamentar = nomeParlamentar
                };
        }

        protected DeputadoEstadual GetDeputadoByMatriculaOrNew(uint matricula)
        {
            return connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    matricula
                })
                .FirstOrDefault()
                ?? new DeputadoEstadual()
                {
                    IdEstado = (ushort)config.Estado,
                    Matricula = matricula
                };
        }

        public void InsertOrUpdate(DeputadoEstadual deputado)
        {
            if (deputado.Id == 0)
                connection.Insert(deputado);
            else
                connection.Update(deputado);
        }
    }

    public class ImportadorParlamentarConfig
    {
        public string BaseAddress { get; set; }

        public Estado Estado { get; set; }

    }
}
