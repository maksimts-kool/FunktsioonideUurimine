import { Box, type BoxProps } from '@mantine/core'
import katex from 'katex'
import { useMemo } from 'react'

interface ValemProps extends BoxProps {
  /** LaTeX, mille genereerib backend (MathNet.Symbolics + ValemiVormindaja). */
  latex: string | null | undefined
  /** Kui LaTeX puudub, kuvatakse see tekst. */
  varuTekst?: string
}

/** Valemi kuvamine KaTeX-iga. */
export function Valem({ latex, varuTekst, ...boxProps }: ValemProps) {
  const html = useMemo(
    () => (latex ? katex.renderToString(latex, { throwOnError: false, strict: 'ignore' }) : null),
    [latex],
  )
  if (!html) return <Box component="span" ff="monospace" {...boxProps}>{varuTekst}</Box>
  return <Box component="span" {...boxProps} dangerouslySetInnerHTML={{ __html: html }} />
}
