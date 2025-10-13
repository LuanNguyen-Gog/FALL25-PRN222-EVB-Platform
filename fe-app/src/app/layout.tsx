import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "EVB Platform - Nền tảng giao dịch xe điện",
  description: "Nền tảng giao dịch xe điện và pin xe điện hàng đầu",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi">
      <body className="min-h-screen flex flex-col">
        {children}
      </body>
    </html>
  );
}
