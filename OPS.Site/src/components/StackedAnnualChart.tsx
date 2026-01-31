import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { formatCurrency, formatValue } from "@/lib/utils";
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend, CartesianGrid } from "recharts";

interface StackedChartData {
  year: string;
  ceaps: number;
  remuneracao: number;
}

interface StackedAnnualChartWithCardProps {
  title: string;
  subtitle: string;
  data: StackedChartData[];
}

interface StackedAnnualChartProps {
  data: StackedChartData[];
}

export const StackedAnnualChart = ({ data }: StackedAnnualChartProps) => {
  return (
    <ResponsiveContainer width="100%" height={300}>
      <BarChart
        data={data}
        margin={{ top: 20, right: 10, left: 0, bottom: 0 }}
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
          formatter={(value: number, name: string) => [
            formatCurrency(value),
            name === 'ceaps' ? 'Cota Parlamentar' : 'Folha de Pagamento'
          ]}
          contentStyle={{
            backgroundColor: 'hsl(var(--card))',
            border: '1px solid hsl(var(--border))',
            borderRadius: '12px',
            boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)',
            textAlign: 'right',
          }}
          cursor={{ fill: 'hsl(var(--muted))', opacity: 0.4 }}
        />
        <Legend
          formatter={(value) => value === 'ceaps' ? 'Cota Parlamentar' : 'Folha de Pagamento'}
          wrapperStyle={{
            paddingTop: '20px',
            fontSize: '11px',
            fontWeight: 700,
            textTransform: 'uppercase',
            letterSpacing: '0.05em'
          }}
        />
        <Bar dataKey="ceaps" stackId="a" fill="hsl(var(--chart-1))" radius={[0, 0, 0, 0]} className="transition-all duration-300 hover:opacity-80" />
        <Bar dataKey="remuneracao" stackId="a" fill="hsl(var(--chart-2))" radius={[4, 4, 0, 0]} className="transition-all duration-300 hover:opacity-80" />
      </BarChart>
    </ResponsiveContainer>
  )
}

export const StackedAnnualChartWithCard = ({ title, subtitle, data }: StackedAnnualChartWithCardProps) => {
  return (
    <Card className="h-full">
      <CardHeader className="pb-2">
        <CardTitle className="text-lg font-semibold">
          {title} <span className="font-normal text-muted-foreground">({subtitle})</span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <StackedAnnualChart data={data} />
      </CardContent>
    </Card>
  );
};
