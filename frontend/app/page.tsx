"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { LoadingBlock } from "@/components/ui";

export default function Home() {
  const { utente, caricato } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!caricato) return;
    router.replace(utente ? "/dashboard" : "/login");
  }, [caricato, utente, router]);

  return <LoadingBlock />;
}
