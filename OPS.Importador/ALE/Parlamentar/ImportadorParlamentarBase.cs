using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

        public int registrosInseridos { get; private set; } = 0;
        public int registrosAtualizados { get; private set; } = 0;

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

        public virtual Task DownloadFotos()
        {
            return Task.CompletedTask;
        }

        public ushort BuscarIdPartido(string partido)
        {
            if (partido == "PATRI" || partido.Equals("PATRIOTAS", StringComparison.InvariantCultureIgnoreCase)) partido = "PATRIOTA";
            else if (partido == "PTC") partido = "AGIR"; // https://agir36.com.br/sobre-o-partido/
            else if (partido == "REPUB" || partido == "REP" || partido == "REPUBLICAN" || partido == "PRB") partido = "REPUBLICANOS";
            else if (partido == "PR") partido = "PL"; // Partido da República
            else if (partido == "Podemos" || partido == "POD") partido = "PODE";
            else if (partido == "UNIÃO BRASIL (UNIÃO)" || partido == "UB") partido = "UNIÃO";
            else if (partido == "CIDA" || partido == "CDN" || partido == "PPS") partido = "CIDADANIA";
            else if (partido == "PSDC") partido = "DC"; // Democracia Cristã
            else if (partido == "PTR") partido = "PP"; // Progressistas
            else if (partido.Equals("PC DO B", StringComparison.InvariantCultureIgnoreCase)) partido = "PCdoB";
            else if (partido.Contains("PROGRESSISTA") || partido == "Partido Progressista") partido = "PP"; // Progressistas
            else if (partido.Contains("SOLIDARIEDADE") || partido == "SDD") partido = "SD"; // Solidariedade
            else if (partido.Contains("PARTIDO VERDE")) partido = "PV";

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
            var deputado = connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    nome_parlamentar = nomeParlamentar
                })
                .FirstOrDefault();

            if (deputado == null)
                deputado = connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    nome_importacao = nomeParlamentar
                })
                .FirstOrDefault();

            if (deputado != null)
                return deputado;

            return new DeputadoEstadual()
            {
                IdEstado = (ushort)config.Estado,
                NomeParlamentar = nomeParlamentar
            };
        }

        protected DeputadoEstadual GetDeputadoByFullNameOrNew(string nome)
        {
            var deputado = connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    nome_civil = nome
                })
                .FirstOrDefault();

            if (deputado != null)
                return deputado;

            return new DeputadoEstadual()
            {
                IdEstado = (ushort)config.Estado,
                NomeCivil = nome
            };
        }

        protected DeputadoEstadual GetDeputadoByMatriculaOrNew(uint matricula)
        {
            var deputado = connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = config.Estado,
                    matricula
                })
                .FirstOrDefault();

            if (deputado != null)
                return deputado;

            return new DeputadoEstadual()
            {
                IdEstado = (ushort)config.Estado,
                Matricula = matricula
            };
        }

        public void InsertOrUpdate(DeputadoEstadual deputado)
        {
            if (deputado.Id == 0)
            {
                connection.Insert(deputado);
                registrosInseridos++;
            }
            else
            {
                connection.Update(deputado);
                registrosAtualizados++;
            }
        }
    }

    public class ImportadorParlamentarConfig
    {
        public string BaseAddress { get; set; }

        public Estado Estado { get; set; }

    }
}
