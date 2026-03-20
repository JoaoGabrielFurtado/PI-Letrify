'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';

interface Grupo {
    id: string;
    nome: string;
    descricao: string;
    membros: number;
    criadoEm: string;
}

export default function GruposPage() {
    const router = useRouter();
    const [grupos, setGrupos] = useState<Grupo[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [matchResults, setMatchResults] = useState<Array<{usuario: {id:number, nome:string, cidade:string, fotoPerfil?:string}, scoreMatch:number}>>([]);
    const [matchLoading, setMatchLoading] = useState(false);
    const [matchError, setMatchError] = useState<string | null>(null);

    const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

    useEffect(() => {
        // O backend não tem rota /api/match/grupos; manter a área de grupos com dados locais se necessário.
        setLoading(false);
        setGrupos([]);
    }, []);

    const fetchGrupos = async () => {
        // Fallback: não há rota de grupos no MatchController, então não chama API por enquanto
        setGrupos([]);
        setLoading(false);
    };

    const fetchMatches = async () => {
        try {
            setMatchLoading(true);
            setMatchError(null);
            setMatchResults([]);

            const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
            const response = await fetch(`${apiBaseUrl}/api/match`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(token ? { Authorization: `Bearer ${token}` } : {})
                }
            });

            if (response.status === 401 || response.status === 403) {
                throw new Error('Autenticação necessária para buscar matches. Faça login.');
            }

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Falha ao buscar matches (${response.status}): ${errorText}`);
            }

            const data = await response.json();
            const usuarios = (data.usuariosParecidos || []).map((item:any) => ({
                usuario: item.usuario,
                scoreMatch: item.scoreMatch
            }));

            setMatchResults(usuarios);
        } catch (err) {
            setMatchError(err instanceof Error ? err.message : 'Erro desconhecido');
        } finally {
            setMatchLoading(false);
        }
    };

    if (loading) return <div className="p-4">Carregando...</div>;
    if (error) return <div className="p-4 text-red-500">Erro: {error}</div>;

    return (
        <div className="p-6">
            <h1 className="text-3xl font-bold mb-4">Grupos</h1>

            <div className="mb-6">
                <button
                    className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition"
                    onClick={fetchMatches}
                    disabled={matchLoading}
                >
                    {matchLoading ? 'Executando match...' : 'Executar match'}
                </button>

                {matchError && (
                    <div className="mt-3 text-red-500">Erro de match: {matchError}</div>
                )}

                {matchResults.length > 0 && (
                    <div className="mt-4 p-4 border rounded bg-gray-50">
                        <h2 className="text-xl font-semibold mb-2">5 melhores matches</h2>
                        <ul className="space-y-2">
                            {matchResults.map((match, i) => (
                                <li key={`${match.usuario.id}-${i}`} className="p-2 border rounded">
                                    <div className="font-semibold">{match.usuario.nome}</div>
                                    <div className="text-sm text-gray-600">{match.usuario.cidade || 'Cidade não informada'}</div>
                                    <div className="text-sm text-gray-500">Score: {match.scoreMatch.toFixed(3)}</div>
                                </li>
                            ))}
                        </ul>
                    </div>
                )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {grupos.map((grupo) => (
                    <div
                        key={grupo.id}
                        className="border rounded-lg p-4 hover:shadow-lg cursor-pointer transition"
                        onClick={() => router.push(`/grupos/${grupo.id}`)}
                    >
                        <h2 className="text-xl font-semibold">{grupo.nome}</h2>
                        <p className="text-gray-600 text-sm mt-2">{grupo.descricao}</p>
                        <p className="text-gray-500 text-xs mt-4">
                            {grupo.membros} membros • {new Date(grupo.criadoEm).toLocaleDateString('pt-BR')}
                        </p>
                    </div>
                ))}
            </div>
        </div>
    );
}