/** Sündmus, millega vastuste tabel palub lahenduskäigu osa avada ja selleni kerida. */
export const AVA_SUNDMUS = 'ava-lahendus'

export const lahenduseId = (voti: string) => `lahendus-${voti}`

/** Avab lahenduskäigu osa ja kerib selleni (kutsutakse vastuste tabelist). */
export function naitaLahendust(voti: string) {
  window.dispatchEvent(new CustomEvent(AVA_SUNDMUS, { detail: voti }))
  requestAnimationFrame(() =>
    document.getElementById(lahenduseId(voti))?.scrollIntoView({ behavior: 'smooth', block: 'start' }),
  )
}
