import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency, formatValue } from "@/lib/utils";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from "recharts";

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
        margin={{ top: 0, right: 10, left: 0, bottom: 0 }}
      >
        <XAxis
          dataKey="year"
          type="category"
          axisLine={false}
          tickLine={false}
          tick={{ fontSize: 11, fill: 'hsl(var(--foreground))' }}
        />
        <YAxis
          type="number"
          tickFormatter={formatValue}
          axisLine={false}
          tickLine={false}
          tick={{ fontSize: 11, fill: 'hsl(var(--muted-foreground))' }}
        />
        <Tooltip
          formatter={(value: number) => [formatCurrency(value), 'Valor']}
          contentStyle={{
            backgroundColor: 'hsl(var(--card))',
            border: '1px solid hsl(var(--border))',
            borderRadius: '8px',
          }}
        />
        <Bar dataKey="value" radius={[0, 4, 4, 0]}>
          {data.map((_, index) => (
            <Cell
              key={`cell-${index}`}
              fill={`hsl(210, ${60 + index * 2}%, ${45 + index}%)`}
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
