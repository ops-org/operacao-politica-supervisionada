import * as React from "react"
import { Slot } from "@radix-ui/react-slot"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
    "inline-flex items-center justify-center whitespace-nowrap rounded-[0.75rem] text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50",
    {
        variants: {
            variant: {
                default: "bg-primary text-primary-foreground hover:bg-primary/90",
                destructive:
                    "bg-destructive text-destructive-foreground hover:bg-destructive/90",
                outline:
                    "border border-input bg-background hover:bg-accent hover:text-accent-foreground",
                secondary:
                    "bg-secondary text-secondary-foreground hover:bg-secondary/80",
                ghost: "hover:bg-accent hover:text-accent-foreground",
                link: "text-primary underline-offset-4 hover:underline",
                gradient: "bg-gradient-to-r from-primary via-purple-500 to-accent text-white shadow-lg shadow-primary/25 hover:shadow-xl hover:shadow-primary/40 hover:-translate-y-0.5 transition-all duration-300 border-0",
                "gradient-secondary": "bg-gradient-to-r from-secondary via-orange-500 to-amber-500 text-white shadow-lg shadow-secondary/25 hover:shadow-xl hover:shadow-secondary/40 hover:-translate-y-0.5 transition-all duration-300 border-0",
                "gradient-outline": "border-2 border-transparent bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent hover:bg-clip-padding hover:text-white hover:bg-gradient-to-r hover:from-primary hover:to-accent transition-all duration-300 relative before:absolute before:inset-0 before:-z-10 before:p-[2px] before:bg-gradient-to-r before:from-primary before:to-accent before:rounded-[0.75rem] before:content-[''] before:mask-composite-exclude bg-white dark:bg-card",
            },
            size: {
                default: "h-10 px-4 py-2",
                sm: "h-9 rounded-[0.6rem] px-3",
                lg: "h-11 rounded-[0.85rem] px-8",
                icon: "h-10 w-10",
            },
        },
        defaultVariants: {
            variant: "default",
            size: "default",
        },
    }
)

export interface ButtonProps
    extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
    asChild?: boolean
}

const GradientButton = React.forwardRef<HTMLButtonElement, ButtonProps>(
    ({ className, variant, size, asChild = false, ...props }, ref) => {
        const Comp = asChild ? Slot : "button"
        return (
            <Comp
                className={cn(buttonVariants({ variant, size, className }))}
                ref={ref}
                {...props}
            />
        )
    }
)
GradientButton.displayName = "Button"

export { GradientButton, buttonVariants }
