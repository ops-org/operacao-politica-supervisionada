CREATE TABLE deputado_campeao_gasto (
	idParlamentar	BIGINT,
	nomeParlamentar VARCHAR(100),
	vlrTotal 		DECIMAL (10, 2),
	sgPartido 		VARCHAR(10),
	sgUf 			VARCHAR(2)
);

CREATE TABLE senador_campeao_gasto (
	idParlamentar	BIGINT,
	nomeParlamentar VARCHAR(100),
	vlrTotal 		DECIMAL (10, 2),
	sgPartido 		VARCHAR(10),
	sgUf 			VARCHAR(2)
);

insert into deputado_campeao_gasto
SELECT l.ideCadastro as Id, p.txNomeParlamentar as NomeParlamentar, sum(l.vlrLiquido) as ValorTotal, p.Partido, p.Uf 
FROM lancamentos l 
INNER JOIN parlamentares p on p.ideCadastro = l.ideCadastro 
where l.AnoMes >= 201502 
group by 1, 2 
order by 3 desc 
limit 4; 

insert into senador_campeao_gasto
SELECT l.CodigoParlamentar as Id, s.NomeParlamentar as NomeParlamentar, sum(l.Valor) as ValorTotal, s.SiglaPartido as Partido, s.siglaUf as Uf 
FROM lancamentos_senadores l 
INNER JOIN senadores s on s.CodigoParlamentar = l.CodigoParlamentar 
where l.AnoMes >= 201002 
group by 1, 2 
order by 3 desc 
limit 4; 

