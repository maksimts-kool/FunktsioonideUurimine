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
