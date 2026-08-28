"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { Button, Field, Input } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";

export default function LoginPage() {
  const { utente, caricato, login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errore, setErrore] = useState<string | null>(null);
  const [inviando, setInviando] = useState(false);

  useEffect(() => {
    if (caricato && utente) router.replace("/dashboard");
  }, [caricato, utente, router]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrore(null);
    setInviando(true);
    try {
      await login(email, password);
      router.replace("/dashboard");
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50 px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          {/* eslint-disable-next-line @next/next/no-img-element -- SVG locale, l'ottimizzazione next/image è superflua */}
          <img src="/logo-admgroup.svg" alt="ADM Group" className="mx-auto h-20 w-auto" />
          <h1 className="mt-2 text-xl font-bold text-teal-800">ADMcrm</h1>
          <p className="mt-1 text-sm text-zinc-500">Accedi al gestionale della rete vendita</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4 rounded-lg border border-zinc-200 bg-white p-6 shadow-sm">
          <Field label="Email">
            <Input
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </Field>
          <Field label="Password">
            <Input
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </Field>

          {errore && <p className="text-sm text-red-600">{errore}</p>}

          <Button type="submit" disabled={inviando} className="w-full">
            {inviando ? "Accesso in corso…" : "Accedi"}
          </Button>
        </form>
      </div>
    </div>
  );
}
