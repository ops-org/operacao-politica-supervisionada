import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency, formatValue } from "@/lib/utils";
import { ComposedChart, Bar, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from "recharts";
import { useIsMobile } from "@/hooks/use-mobile";

interface DadosGrafico {
  ano: string;
  valor: number;
  valor_deflacionado?: number;
}

interface PropsGraficoResumoAnualComCard {
  titulo: string;
  subtitulo: string;
  dados: DadosGrafico[];
}

interface PropsGraficoResumoAnual {
  dados: DadosGrafico[];
}


export const GraficoResumoAnual = ({ dados }: PropsGraficoResumoAnual) => {
  const isMobile = useIsMobile();

  // No mobile, aumentamos a altura com base no número de itens para evitar compressão
  const alturaGrafico = isMobile ? Math.max(300, dados.length * 60) : 300;

  return (
    <ResponsiveContainer width="100%" height={alturaGrafico}>
      <ComposedChart
        data={dados}
        layout={isMobile ? "vertical" : "horizontal"}
        margin={{
          top: 20,
          right: isMobile ? 40 : 30,
          left: isMobile ? 10 : 20,
          bottom: 5
        }}
      >
        <CartesianGrid
          strokeDasharray="3 3"
          vertical={!isMobile}
          horizontal={isMobile}
          stroke="hsl(var(--muted))"
        />

        {isMobile ? (
          <>
            <XAxis
              type="number"
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={formatValue}
            />
            <YAxis
              dataKey="ano"
              type="category"
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              width={50}
            />
          </>
        ) : (
          <>
            <XAxis
              dataKey="ano"
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
            />
            <YAxis
              stroke="hsl(var(--muted-foreground))"
              fontSize={12}
              tickLine={false}
              axisLine={false}
              tickFormatter={formatValue}
              domain={[0, 'dataMax']}
            />
          </>
        )}

        <Tooltip
          content={({ active, payload, label }) => {
            if (active && payload && payload.length) {
              return (
                <div className="rounded-lg border bg-background p-3 shadow-md backdrop-blur-sm border-primary/20">
                  <div className="flex flex-col mb-2">
                    <span className="text-[0.70rem] uppercase text-muted-foreground font-semibold">
                      Ano
                    </span>
                    <span className="font-bold text-primary">
                      {label}
                    </span>
                  </div>
                  <div className="space-y-2">
                    {payload.map((entrada, index) => (
                      <div key={`item-${index}`} className="flex items-center gap-3">
                        <div
                          className="h-3 w-3 rounded-full shadow-sm"
                          style={{ backgroundColor: entrada.color }}
                        />
                        <div className="flex flex-col">
                          <span className="text-[0.65rem] uppercase text-muted-foreground leading-tight">
                            {entrada.name}
                          </span>
                          <span className="font-bold tabular-nums">
                            {formatCurrency(entrada.value as number)}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              );
            }
            return null;
          }}
          cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
        />
        <Legend
          wrapperStyle={{ fontSize: '12px', paddingTop: '20px' }}
          verticalAlign="bottom"
          align="center"
        />
        <Bar
          name="Valor Original"
          dataKey="valor"
          fill="hsl(var(--primary))"
          radius={isMobile ? [0, 4, 4, 0] : [4, 4, 0, 0]}
          barSize={isMobile ? 30 : 40}
          opacity={0.8}
        />
        <Area
          type="monotone"
          name="Deflacionado (IPCA)"
          dataKey="valor_deflacionado"
          fill="hsl(var(--accent))"
          fillOpacity={0.2}
          stroke="hsl(var(--accent))"
          strokeWidth={2}
          connectNulls
        />
      </ComposedChart>
    </ResponsiveContainer>
  )
}

export const GraficoResumoAnualComCard = ({ titulo, subtitulo, dados }: PropsGraficoResumoAnualComCard) => {
  return (
    <Card className="h-full border-0 shadow-none bg-transparent">
      <CardHeader className="pb-4 px-6 pt-6">
        <CardTitle className="text-xl font-bold flex flex-col sm:flex-row sm:items-baseline gap-1">
          {titulo}
          <span className="text-sm font-normal text-muted-foreground">({subtitulo})</span>
        </CardTitle>
      </CardHeader>
      <CardContent className="px-2 sm:px-6 pb-6 mt-1">
        <GraficoResumoAnual dados={dados} />
      </CardContent>
    </Card>
  );
};

