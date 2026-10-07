import type {
  AnaluusiVastus,
  FunktsiooniPaering,
  FunktsiooniVastus,
  GraafikuPaering,
  GraafikuVastus,
  LahenduskaiguVastus,
} from './tyybid'

/** Serveri viga; `valjad` sisaldab valideerimisvigu väljade kaupa (ASP.NET ValidationProblemDetails). */
export class ApiViga extends Error {
  readonly status: number
  readonly valjad: Record<string, string>

  constructor(status: number, sonum: string, valjad: Record<string, string> = {}) {
    super(sonum)
    this.status = status
    this.valjad = valjad
  }
}

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function paring<T>(tee: string, init?: RequestInit): Promise<T> {
  const vastus = await fetch(`/api${tee}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', Accept: 'application/json', ...init?.headers },
  })

  if (!vastus.ok) {
    const probleem: ProblemDetails | null = await vastus.json().catch(() => null)
    // "Valem" → "valem", et vastaks vormi väljadele
    const valjad = Object.fromEntries(
      Object.entries(probleem?.errors ?? {}).map(([k, v]) => [k.charAt(0).toLowerCase() + k.slice(1), v.join(' ')]),
    )
    const sonum =
      Object.values(valjad)[0] ??
      probleem?.detail ??
      (vastus.status === 404 ? 'Funktsiooni ei leitud.' : `Serveri viga (${vastus.status}).`)
    throw new ApiViga(vastus.status, sonum, valjad)
  }

  return vastus.status === 204 ? (undefined as T) : ((await vastus.json()) as T)
}

const json = (keha: unknown) => JSON.stringify(keha)

export const api = {
  funktsioonid: () => paring<FunktsiooniVastus[]>('/funktsioonid'),
  funktsioon: (id: number) => paring<FunktsiooniVastus>(`/funktsioonid/${id}`),
  loo: (p: FunktsiooniPaering) => paring<FunktsiooniVastus>('/funktsioonid', { method: 'POST', body: json(p) }),
  muuda: (id: number, p: FunktsiooniPaering) =>
    paring<FunktsiooniVastus>(`/funktsioonid/${id}`, { method: 'PUT', body: json(p) }),
  kustuta: (id: number) => paring<void>(`/funktsioonid/${id}`, { method: 'DELETE' }),
  analuus: (p: FunktsiooniPaering, signal?: AbortSignal) =>
    paring<AnaluusiVastus>('/analuus', { method: 'POST', body: json(p), signal }),
  lahenduskaik: (p: FunktsiooniPaering, signal?: AbortSignal) =>
    paring<LahenduskaiguVastus>('/lahenduskaik', { method: 'POST', body: json(p), signal }),
  graafik: (p: GraafikuPaering, signal?: AbortSignal) =>
    paring<GraafikuVastus>('/graafik', { method: 'POST', body: json(p), signal }),
}
