"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { Sidebar } from "@/components/Sidebar";
import { LoadingBlock } from "@/components/ui";

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { utente, caricato } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (caricato && !utente) router.replace("/login");
  }, [caricato, utente, router]);

  if (!caricato || !utente) {
    return <LoadingBlock label="Verifica sessione…" />;
  }

  return (
    <div className="flex">
      <Sidebar />
      <main className="min-h-screen flex-1 overflow-x-hidden px-8 py-6">{children}</main>
    </div>
  );
}
