import type { NextConfig } from "next";

// In produzione il browser chiama /api/* sulla stessa origine di Vercel (nessun CORS
// necessario) e Vercel inoltra internamente le richieste al backend su Render: alcuni header
// CORS impostati dall'app .NET vengono persi lungo l'infrastruttura di Render, questo rewrite
// evita del tutto il problema perché il browser non fa mai una richiesta cross-origin.
const BACKEND_URL = process.env.BACKEND_URL ?? "https://admcrm-api.onrender.com";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${BACKEND_URL}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
