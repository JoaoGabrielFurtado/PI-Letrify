import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import ThemeManager from "@/components/ThemeManager";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Letrify | Sua rede social literária",
  description: "Conecte-se com leitores, descubra livros e compartilhe suas resenhas.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="pt-BR">
      <body>  
        <ThemeManager />
        {children}
      </body>
    </html>
  );
}


