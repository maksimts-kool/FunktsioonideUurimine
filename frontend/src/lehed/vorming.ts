const kuupaevaVorming = new Intl.DateTimeFormat('et-EE', { dateStyle: 'medium', timeStyle: 'short' })

export const kuupaev = (iso: string) => kuupaevaVorming.format(new Date(iso))
