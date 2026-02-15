import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency, formatValue } from "@/lib/utils";
import { ComposedChart, Bar, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from "recharts";

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
  return (
    <ResponsiveContainer width="100%" height={300}>
      <ComposedChart data={dados} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="hsl(var(--muted))" />
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
        <Tooltip
          content={({ active, payload, label }) => {
            if (active && payload && payload.length) {
              return (
                <div className="rounded-lg border bg-background p-2 shadow-sm">
                  <div className="grid grid-cols-2 gap-2">
                    <div className="flex flex-col">
                      <span className="text-[0.70rem] uppercase text-muted-foreground">
                        Ano
                      </span>
                      <span className="font-bold text-muted-foreground">
                        {label}
                      </span>
                    </div>
                  </div>
                  <div className="mt-2 space-y-1">
                    {payload.map((entrada, index) => (
                      <div key={`item-${index}`} className="flex items-center gap-2">
                        <div
                          className="h-2 w-2 rounded-full"
                          style={{ backgroundColor: entrada.color }}
                        />
                        <div className="flex flex-col">
                          <span className="text-[0.70rem] uppercase text-muted-foreground">
                            {entrada.name}
                          </span>
                          <span className="font-bold">
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
          wrapperStyle={{ fontSize: '12px', paddingTop: '10px' }}
        />
        <Area
          type="monotone"
          name="Deflacionado (IPCA)"
          dataKey="valor_deflacionado"
          fill="hsl(var(--primary))"
          fillOpacity={0.2}
          stroke="hsl(var(--primary))"
          strokeWidth={2}
        />
        <Bar
          name="Valor Original"
          dataKey="valor"
          fill="hsl(var(--primary))"
          radius={[4, 4, 0, 0]}
          barSize={40}
          opacity={0.8}
        />
      </ComposedChart>
    </ResponsiveContainer>
  )
}

export const GraficoResumoAnualComCard = ({ titulo, subtitulo, dados }: PropsGraficoResumoAnualComCard) => {
  return (
    <Card className="h-full">
      <CardHeader className="pb-2">
        <CardTitle className="text-lg font-semibold">
          {titulo} <span className="font-normal text-muted-foreground">({subtitulo})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <GraficoResumoAnual dados={dados} />
      </CardContent>
    </Card>
  );
};
