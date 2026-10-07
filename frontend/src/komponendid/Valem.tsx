import { Box, type BoxProps } from '@mantine/core'
import katex from 'katex'
import { Fragment, useMemo } from 'react'
import klassid from './Valem.module.css'

interface ValemProps extends BoxProps {
  /** LaTeX, mille genereerib backend (MathNet.Symbolics + ValemiVormindaja). */
  latex: string | null | undefined
  /** Kui LaTeX puudub, kuvatakse see tekst. */
  varuTekst?: string
  /** Eraldi real kuvatav (display-režiimis) valem. */
  blokk?: boolean
}

/** Valemi kuvamine KaTeX-iga. */
export function Valem({ latex, varuTekst, blokk = false, ...boxProps }: ValemProps) {
  const html = useMemo(
    () => (latex ? katex.renderToString(latex, { throwOnError: false, strict: 'ignore', displayMode: blokk }) : null),
    [latex, blokk],
  )
  if (!html) return <Box component="span" ff="monospace" {...boxProps}>{varuTekst}</Box>
  return (
    <Box
      component={blokk ? 'div' : 'span'}
      className={blokk ? klassid.blokk : undefined}
      {...boxProps}
      dangerouslySetInnerHTML={{ __html: html }}
    />
  )
}

/** Tekst, milles $…$ on valem ja **…** paks kiri (lahenduskäigu ja teooria lehe sammud). */
export function RikasTekst({ tekst }: { tekst: string }) {
  return tekst.split(/(\*\*[\s\S]+?\*\*)/).map((osa, i) =>
    osa.startsWith('**') && osa.endsWith('**') ? (
      <strong key={i}><Valemitega tekst={osa.slice(2, -2)} /></strong>
    ) : (
      <Valemitega key={i} tekst={osa} />
    ),
  )
}

function Valemitega({ tekst }: { tekst: string }) {
  return tekst.split(/(\$[^$]+\$)/).map((osa, i) =>
    osa.length > 2 && osa.startsWith('$') && osa.endsWith('$') ? (
      <Valem key={i} latex={osa.slice(1, -1)} />
    ) : (
      <Fragment key={i}>{osa}</Fragment>
    ),
  )
}
