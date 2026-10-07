// Backendi DTO-de (Mudelid/Dto.cs) TypeScripti vasted.

export interface FunktsiooniPaering {
  valem: string
  vahemikAlgus: number
  vahemikLopp: number
}

export interface GraafikuPaering {
  valem: string
  algus: number
  lopp: number
  samm: number
}

export interface Omadused {
  valem: string
  valemLatex: string | null
  maaramispiirkond: string
  nullkohad: string
  positiivsus: string
  tuletis: string
  tuletisLatex: string | null
  kriitilisedPunktid: string
  ekstreemumid: string
  monotoonsus: string
  teineTuletis: string
  teineTuletisLatex: string | null
  kaanupunktid: string
  kumerus: string
  vahemikAlgus: number
  vahemikLopp: number
}

export interface AnaluusiVastus extends Omadused {
  numbriline: boolean
}

export interface FunktsiooniVastus extends Omadused {
  id: number
  luodudAeg: string
  muudetudAeg: string | null
}

export interface Punkt {
  x: number
  y: number
}

export interface Ekstreemum extends Punkt {
  tyyp: 'max' | 'min'
}

export interface GraafikuVastus {
  x: number[]
  y: (number | null)[]
  yTuletis: (number | null)[]
  yTeineTuletis: (number | null)[]
  nullkohad: Punkt[]
  ekstreemumid: Ekstreemum[]
  kaanupunktid: Punkt[]
  asumptoodid: number[]
  valemLatex: string
  tuletisLatex: string
  teineTuletisLatex: string
}

/** Märgitabeli veerg: vahemik (märk testpunktist) või punkt (0 / ∄). LaTeX-väljad renderdab KaTeX. */
export interface TabeliVeerg {
  x: string
  vahemik: boolean
  /** "+", "−", "0" või "∄" */
  mark: string
  tahendus: string | null
  /** testpunkti arvutus, nt "f'(-2) = 9" */
  kontroll: string | null
}

export interface Margitabel {
  margiRida: string
  tahenduseRida: string | null
  veerud: TabeliVeerg[]
}

/** Lahenduskäigu samm: tekst ($…$ – valem, **…** – paks kiri), eraldi real valem ja/või märgitabel. */
export interface Samm {
  tekst: string | null
  valem: string | null
  tabel: Margitabel | null
}

export interface LahenduseOsa {
  /** vastab vastuste tabeli reale, nt "nullkohad" */
  voti: string
  pealkiri: string
  sammud: Samm[]
  vastused: string[]
  numbriline: boolean
}

export interface LahenduskaiguVastus {
  valem: string
  valemLatex: string
  vahemikAlgus: number
  vahemikLopp: number
  osad: LahenduseOsa[]
}
