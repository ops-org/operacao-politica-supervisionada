import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency, formatValue } from "@/lib/utils";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell, CartesianGrid } from "recharts";

interface ChartData {
  year: string;
  value: number;
}

interface AnnualSummaryChartWithCardProps {
  title: string;
  subtitle: string;
  data: ChartData[];
}

interface AnnualSummaryChartProps {
  data: ChartData[];
}


export const AnnualSummaryChart = ({ data }: AnnualSummaryChartProps) => {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart
        data={data}
        margin={{ top: 10, right: 10, left: 0, bottom: 0 }}
      >
        <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" vertical={false} />
        <XAxis
          dataKey="year"
          type="category"
          axisLine={false}
          tickLine={false}
          tick={{ fontSize: 11, fill: 'hsl(var(--muted-foreground))' }}
          dy={10}
        />
        <YAxis
          type="number"
          tickFormatter={formatValue}
          axisLine={false}
          tickLine={false}
          tick={{ fontSize: 11, fill: 'hsl(var(--muted-foreground))' }}
        />
        <Tooltip
          content={({ active, payload, label }) => {
            if (active && payload && payload.length) {
              return (
                <div
                  style={{
                    backgroundColor: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '12px',
                    boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)',
                    padding: '8px 12px',
                    color: 'hsl(var(--foreground))'
                  }}
                >
                  <div style={{ fontSize: '12px', marginBottom: '4px', opacity: 0.8 }}>
                    Ano: {label}
                  </div>
                  <div style={{ fontSize: '12px', opacity: 0.8 }}>
                    Valor
                  </div>
                  <div style={{ fontWeight: 'bold' }}>
                    {formatCurrency(payload[0].value as number)}
                  </div>
                  
                </div>
              );
            }
            return null;
          }}
          cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
        />
        <Bar dataKey="value" radius={[4, 4, 0, 0]}>
          {data.map((_, index) => (
            <Cell
              key={`cell-${index}`}
              fill={index === data.length - 1 ? 'hsl(var(--primary))' : `hsl(var(--primary) / ${0.3 + (index / data.length) * 0.7})`}
              className="transition-all duration-300 hover:opacity-80"
            />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  )
}

export const AnnualSummaryChartWithCard = ({ title, subtitle, data }: AnnualSummaryChartWithCardProps) => {
  return (
    <Card className="h-full">
      <CardHeader className="pb-2">
        <CardTitle className="text-lg font-semibold">
          {title} <span className="font-normal text-muted-foreground">({subtitle})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <AnnualSummaryChart data={data} />
      </CardContent>
    </Card>
  );
};
