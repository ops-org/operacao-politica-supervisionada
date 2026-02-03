import { Card, CardContent } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Link } from "react-router-dom";
import { MapPin } from "lucide-react";
import { PoliticianType, getImageUrl, getDetailUrl } from "@/types/politician";

interface TopSpenderCardProps {
  name: string;
  party: string;
  state: string;
  amount: string;
  type: PoliticianType;
  id: number;
  ativo?: boolean;
}

export const TopSpenderCard = ({ name, party, state, amount, type, id, ativo = true }: TopSpenderCardProps) => {
  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  };

  return (
    <Link to={getDetailUrl(type, id)} className="group block">
      <Card className={`group relative shadow-lg border-0 bg-card/80 backdrop-blur-sm overflow-hidden transition-all duration-500 hover:shadow-2xl hover:-translate-y-2 cursor-pointer border-t-4 ${ativo ? "border-t-primary/20 hover:border-t-primary" : "border-t-slate-300"}`}>
        {/* Card Header with Status Gradient */}
        <div className={`absolute top-0 left-0 right-0 h-24 z-0 ${ativo
          ? "bg-gradient-to-r from-primary/10 to-accent/5 group-hover:from-primary/20"
          : "bg-gradient-to-r from-slate-500/10 to-transparent"
          }`} />

        {/* Decorative background element */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-full blur-3xl -mr-16 -mt-16 group-hover:bg-primary/10 transition-colors" />

        <CardContent className="flex items-center gap-5 p-5 relative z-10">
          <div className="relative">
            <div className={`absolute -inset-1 bg-gradient-to-br ${ativo ? "from-primary to-accent" : "from-slate-400 to-slate-200"} rounded-2xl blur opacity-20 group-hover:opacity-40 transition duration-500`}></div>
             <Avatar className={`h-32 w-24 rounded-2xl border-2 border-background shadow-xl group-hover:scale-105 transition-all duration-500 relative z-10 ${!ativo ? "grayscale opacity-80" : ""}`}>
              <AvatarImage
                src={getImageUrl(type, id)}
                alt={name}
                onError={(e) => {
                  // Handle CORS errors and other image loading issues
                  console.log(`Image load failed for ${name}:`, getImageUrl(type, id));
                }}
              />
              <AvatarFallback className="rounded-2xl text-xl font-black bg-muted text-muted-foreground uppercase shadow-inner">
                {getInitials(name)}
              </AvatarFallback>
            </Avatar>
          </div>

          <div className="flex-1 min-w-0 space-y-3 pt-1">
            <div>
              <h4 className="font-black text-lg leading-tight text-foreground group-hover:text-primary transition-colors truncate tracking-tight">
                {name}
              </h4>
              <div className="flex items-center gap-1.5 mt-1.5">
                <Badge className="font-black bg-primary/5 text-primary border-primary/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={party}>
                  {party}
                </Badge>
                <Badge variant="outline" className="flex items-center gap-1 font-bold border-muted-foreground/10 text-[10px] uppercase tracking-tighter px-2.5 py-0.5" title={state}>
                  <MapPin className="w-2.5 h-2.5" />
                  {state}
                </Badge>
              </div>
            </div>

            <div className="pt-2 border-t border-border/50">
              <p className="text-[9px] font-black text-muted-foreground uppercase tracking-widest opacity-60">Total Gasto</p>
              <p className="text-xl font-black text-primary font-mono tracking-tighter whitespace-nowrap group-hover:scale-105 transition-transform origin-left">
                {amount}
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
};
