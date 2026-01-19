import { Card, CardContent } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Link } from "react-router-dom";
import { MapPin } from "lucide-react";

interface TopSpenderCardProps {
  name: string;
  party: string;
  state: string;
  amount: string;
  imageUrl?: string;
  type: "deputado-estadual"| "deputado-federal" | "senador";
  id: number;
}

export const TopSpenderCard = ({ name, party, state, amount, imageUrl, type, id }: TopSpenderCardProps) => {
  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  };

  const getDetailUrl = () => {
      return `/${type}/${id}`;
  };

  const getImageUrl = () => {
    if (type === "deputado-federal") {
      return `//static.ops.org.br/depfederal/${id}_120x160.jpg`;
    } else if (type === "deputado-estadual") {
      return null; // `//static.ops.org.br/depestadual/${id}_120x160.jpg`;
    } else {
      return `//static.ops.org.br/senador/${id}_120x160.jpg`;
    }
  };

  return (
    <Link to={getDetailUrl()} className="block">
      <Card className="overflow-hidden transition-shadow hover:shadow-md cursor-pointer">
        <CardContent className="flex items-center gap-3 p-4">
          <Avatar className="h-32 w-24 rounded-xl border-4 border-white shadow-lg group-hover:scale-105 transition-transform">
            <AvatarImage
              src={getImageUrl()}
              alt={name}
            />
            <AvatarFallback className="rounded-xl text-xl font-semibold bg-gradient-to-br from-primary/20 to-primary/10">
              {getInitials(name)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1 min-w-0">
            <h4 className="font-semibold text-foreground truncate mb-2">{name}</h4>
            <div className="flex items-center gap-2 mb-4">
              <Badge variant="secondary" className="font-semibold" title={party}>
                {party}
              </Badge>
              <Badge variant="outline" className="flex items-center gap-1" title={state}>
                <MapPin className="w-3 h-3" />
                {state}
              </Badge>
            </div>
            <p className="text-xl font-bold text-primary">
              {amount}
            </p>
          </div>
        </CardContent>
      </Card>
    </Link>
  );
};
