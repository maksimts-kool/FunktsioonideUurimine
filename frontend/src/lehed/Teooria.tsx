import {
  Anchor,
  Badge,
  Box,
  Button,
  Divider,
  Grid,
  Group,
  List,
  Paper,
  Stack,
  Table,
  TableOfContents,
  Text,
  Title,
} from '@mantine/core'
import { IconArrowRight, IconBook2 } from '@tabler/icons-react'
import { type ReactNode, useEffect } from 'react'
import { Link, useLocation } from 'react-router'
import type { Margitabel } from '../api/tyybid'
import { MargitabelVaade } from '../komponendid/MargitabelVaade'
import { RikasTekst, Valem } from '../komponendid/Valem'

// ------------------------------------------------------------------ ehitusklotsid

const raamatuKiri = "Charter, 'Bitstream Charter', 'Sitka Text', Cambria, Georgia, serif"

/** Lõik raamatukirjas; $…$ on valem ja **…** paks kiri. */
function L({ children }: { children: string }) {
  return (
    <Text component="p" fz={17} lh={1.75} ff={raamatuKiri} my={0}>
      <RikasTekst tekst={children} />
    </Text>
  )
}

/** Eraldi real valem. */
function V({ children }: { children: string }) {
  return <Valem latex={children} blokk fz="lg" style={{ overflowX: 'auto', overflowY: 'hidden' }} />
}

const KASTI_VARV = { definitsioon: 'indigo', teoreem: 'grape', naide: 'teal', tahelepanu: 'orange' } as const
const KASTI_NIMI = { definitsioon: 'Definitsioon', teoreem: 'Teoreem', naide: 'Näide', tahelepanu: 'Tähelepanu' } as const

function Kast({ liik, pealkiri, children }: { liik: keyof typeof KASTI_VARV; pealkiri?: string; children: ReactNode }) {
  const varv = KASTI_VARV[liik]
  return (
    <Paper
      p="md"
      radius="md"
      bg={`var(--mantine-color-${varv}-light)`}
      style={{ borderLeft: `4px solid var(--mantine-color-${varv}-filled)` }}
    >
      <Text size="xs" fw={700} tt="uppercase" c={varv} mb={6} style={{ letterSpacing: 0.6 }}>
        {KASTI_NIMI[liik]}
        {pealkiri && <Text span inherit c="dimmed" fw={600} tt="none"> · {pealkiri}</Text>}
      </Text>
      <Stack gap="xs">{children}</Stack>
    </Paper>
  )
}

/** Link lahendajasse, kus näite valem on juba sisestatud. */
function Proovi({ valem, algus = -5, lopp = 5 }: { valem: string; algus?: number; lopp?: number }) {
  const otsing = new URLSearchParams({ valem, algus: String(algus), lopp: String(lopp) })
  return (
    <Button
      component={Link}
      to={`/uus?${otsing}`}
      variant="light"
      size="compact-sm"
      w="fit-content"
      rightSection={<IconArrowRight size={14} />}
    >
      Proovi lahendajas: {valem}
    </Button>
  )
}

function Peatykk({ id, nr, pealkiri, children }: { id: string; nr: number; pealkiri: string; children: ReactNode }) {
  return (
    <Stack gap="md" component="section">
      <Title order={2} id={id} data-toc="peatykk" ff={raamatuKiri} fw={600} style={{ scrollMarginTop: 80 }}>
        <Text span inherit c="dimmed" fw={400} mr="xs">{nr}.</Text>
        {pealkiri}
      </Title>
      {children}
    </Stack>
  )
}

function Alapealkiri({ id, children }: { id?: string; children: string }) {
  return (
    <Title order={4} id={id} data-toc={id ? 'alapeatykk' : undefined} mt="xs" style={{ scrollMarginTop: 80 }}>
      {children}
    </Title>
  )
}

// ------------------------------------------------------------------ joonised

/** Puutuja: tuletis on puutuja tõus. */
function PuutujaJoonis() {
  return (
    <Box component="figure" m={0}>
      <svg viewBox="0 0 360 200" width="100%" style={{ maxWidth: 420, display: 'block', margin: '0 auto' }} role="img" aria-label="Kõver ja selle puutuja punktis a">
        <line x1="20" y1="180" x2="345" y2="180" stroke="currentColor" strokeOpacity="0.35" />
        <line x1="40" y1="195" x2="40" y2="10" stroke="currentColor" strokeOpacity="0.35" />
        <path d="M 40 170 C 120 165, 200 120, 330 20" fill="none" stroke="var(--mantine-color-indigo-6)" strokeWidth="3" />
        <line x1="90" y1="186" x2="330" y2="58" stroke="var(--mantine-color-orange-6)" strokeWidth="2" strokeDasharray="6 4" />
        <line x1="200" y1="118" x2="290" y2="118" stroke="currentColor" strokeOpacity="0.5" />
        <line x1="290" y1="118" x2="290" y2="70" stroke="currentColor" strokeOpacity="0.5" />
        <circle cx="200" cy="118" r="5" fill="var(--mantine-color-orange-6)" />
        <text x="245" y="134" fontSize="13" fill="currentColor" textAnchor="middle">Δx</text>
        <text x="300" y="98" fontSize="13" fill="currentColor">Δy</text>
        <text x="186" y="108" fontSize="13" fill="currentColor" textAnchor="end">A(a; f(a))</text>
        <text x="320" y="16" fontSize="13" fill="var(--mantine-color-indigo-6)">y = f(x)</text>
        <text x="96" y="168" fontSize="13" fill="var(--mantine-color-orange-6)">puutuja, tõus k = f′(a)</text>
      </svg>
      <Text component="figcaption" size="sm" c="dimmed" ta="center" mt={4}>
        Joonis 1. Tuletis punktis on puutuja tõus: <Valem latex="k = \tan\alpha = f'(a)" />
      </Text>
    </Box>
  )
}

/** Ekstreemumid ja monotoonsus ühel kõveral. */
function EkstreemumiJoonis() {
  return (
    <Box component="figure" m={0}>
      <svg viewBox="0 0 400 190" width="100%" style={{ maxWidth: 460, display: 'block', margin: '0 auto' }} role="img" aria-label="Kõver, millel on maksimum ja miinimum">
        <line x1="10" y1="100" x2="390" y2="100" stroke="currentColor" strokeOpacity="0.35" />
        <path d="M 20 175 C 70 40, 120 35, 150 45 S 230 165, 270 160 S 350 60, 385 20" fill="none" stroke="var(--mantine-color-indigo-6)" strokeWidth="3" />
        <circle cx="128" cy="38" r="5" fill="var(--mantine-color-red-6)" />
        <circle cx="262" cy="161" r="5" fill="var(--mantine-color-teal-6)" />
        <line x1="100" y1="38" x2="156" y2="38" stroke="var(--mantine-color-red-6)" strokeDasharray="4 3" />
        <line x1="234" y1="161" x2="290" y2="161" stroke="var(--mantine-color-teal-6)" strokeDasharray="4 3" />
        <text x="128" y="24" fontSize="13" fill="var(--mantine-color-red-6)" textAnchor="middle">max, f′ = 0</text>
        <text x="262" y="184" fontSize="13" fill="var(--mantine-color-teal-6)" textAnchor="middle">min, f′ = 0</text>
        <text x="78" y="122" fontSize="14" fill="currentColor">f′ &gt; 0 ↗</text>
        <text x="138" y="168" fontSize="14" fill="currentColor">f′ &lt; 0 ↘</text>
        <text x="332" y="160" fontSize="14" fill="currentColor">f′ &gt; 0 ↗</text>
      </svg>
      <Text component="figcaption" size="sm" c="dimmed" ta="center" mt={4}>
        Joonis 2. Maksimumis muutub tuletise märk + → −, miinimumis − → +; puutuja on horisontaalne.
      </Text>
    </Box>
  )
}

/** Nõgus ja kumer kaar ning käänupunkt. */
function KumerusJoonis() {
  return (
    <Box component="figure" m={0}>
      <svg viewBox="0 0 400 170" width="100%" style={{ maxWidth: 460, display: 'block', margin: '0 auto' }} role="img" aria-label="Kumer kaar, käänupunkt ja nõgus kaar">
        <path d="M 20 30 C 60 120, 150 130, 200 85" fill="none" stroke="var(--mantine-color-teal-6)" strokeWidth="3" />
        <path d="M 200 85 C 250 40, 340 50, 380 140" fill="none" stroke="var(--mantine-color-grape-6)" strokeWidth="3" />
        <circle cx="200" cy="85" r="5" fill="var(--mantine-color-orange-6)" />
        <text x="200" y="72" fontSize="13" fill="var(--mantine-color-orange-6)" textAnchor="middle">K – käänupunkt</text>
        <text x="90" y="150" fontSize="13" fill="var(--mantine-color-teal-6)" textAnchor="middle">nõgus ∪, f″ &gt; 0</text>
        <text x="300" y="30" fontSize="13" fill="var(--mantine-color-grape-6)" textAnchor="middle">kumer ∩, f″ &lt; 0</text>
      </svg>
      <Text component="figcaption" size="sm" c="dimmed" ta="center" mt={4}>
        Joonis 3. Käänupunktis muutub kõverus: teine tuletis muudab märki.
      </Text>
    </Box>
  )
}

// ------------------------------------------------------------------ tabelid ja näite andmed

const TULETISED: [string, string][] = [
  ['c', '0'],
  ['x^{n}', 'n x^{n-1}'],
  ['\\sqrt{x}', '\\frac{1}{2\\sqrt{x}}'],
  ['\\frac{1}{x}', '-\\frac{1}{x^{2}}'],
  ['e^{x}', 'e^{x}'],
  ['a^{x}', 'a^{x} \\ln a'],
  ['\\ln x', '\\frac{1}{x}'],
  ['\\log_a x', '\\frac{1}{x \\ln a}'],
  ['\\sin x', '\\cos x'],
  ['\\cos x', '-\\sin x'],
  ['\\tan x', '\\frac{1}{\\cos^{2} x}'],
  ['\\cot x', '-\\frac{1}{\\sin^{2} x}'],
  ['\\arcsin x', '\\frac{1}{\\sqrt{1 - x^{2}}}'],
  ['\\arccos x', '-\\frac{1}{\\sqrt{1 - x^{2}}}'],
  ['\\arctan x', '\\frac{1}{1 + x^{2}}'],
]

const REEGLID: [string, string, string][] = [
  ['Summa ja vahe', "(u \\pm v)' = u' \\pm v'", "(x^{3} - 3x)' = 3x^{2} - 3"],
  ['Konstantne tegur', "(c \\cdot u)' = c \\cdot u'", "(5x^{2})' = 10x"],
  ['Korrutis', "(u v)' = u' v + u v'", "(x^{2}\\ln x)' = 2x \\ln x + x"],
  ['Jagatis', "\\left(\\frac{u}{v}\\right)' = \\frac{u' v - u v'}{v^{2}}", "\\left(\\frac{x}{x + 1}\\right)' = \\frac{1}{(x + 1)^{2}}"],
  ['Liitfunktsioon (ahelreegel)', "\\big(f(g(x))\\big)' = f'(g(x)) \\cdot g'(x)", "(\\sin 3x)' = 3\\cos 3x"],
]

const NAITE_TABEL: Margitabel = {
  margiRida: "f'(x)",
  tahenduseRida: 'f(x)',
  veerud: [
    { x: '(-\\infty;\\ -1)', vahemik: true, mark: '+', tahendus: '\\nearrow', kontroll: "f'(-2) = 9" },
    { x: '-1', vahemik: false, mark: '0', tahendus: '\\text{max}', kontroll: null },
    { x: '(-1;\\ 1)', vahemik: true, mark: '−', tahendus: '\\searrow', kontroll: "f'(0) = -3" },
    { x: '1', vahemik: false, mark: '0', tahendus: '\\text{min}', kontroll: null },
    { x: '(1;\\ \\infty)', vahemik: true, mark: '+', tahendus: '\\nearrow', kontroll: "f'(2) = 9" },
  ],
}

const MARGI_NAIDE: Margitabel = {
  margiRida: 'f(x)',
  tahenduseRida: null,
  veerud: [
    { x: '(-\\infty;\\ -2)', vahemik: true, mark: '+', tahendus: null, kontroll: 'f(-3) = 5' },
    { x: '-2', vahemik: false, mark: '0', tahendus: null, kontroll: null },
    { x: '(-2;\\ 2)', vahemik: true, mark: '−', tahendus: null, kontroll: 'f(0) = -4' },
    { x: '2', vahemik: false, mark: '0', tahendus: null, kontroll: null },
    { x: '(2;\\ \\infty)', vahemik: true, mark: '+', tahendus: null, kontroll: 'f(3) = 5' },
  ],
}

// ------------------------------------------------------------------ leht

export function Teooria() {
  const { hash } = useLocation()

  // /teooria#tuletiste-tabel – keri peatükini ka siis, kui leht avatakse teiselt lehelt
  useEffect(() => {
    if (!hash) return
    const t = window.setTimeout(() => document.getElementById(decodeURIComponent(hash.slice(1)))?.scrollIntoView(), 50)
    return () => window.clearTimeout(t)
  }, [hash])

  return (
    <Grid gap="xl" maw={1240}>
      <Grid.Col span={{ base: 12, lg: 9 }}>
        <Stack gap={40} maw={780} pb={80}>
          <Stack gap="sm">
            <Group gap="xs">
              <IconBook2 size={22} color="var(--mantine-color-indigo-6)" />
              <Text size="sm" c="dimmed" fw={500}>Käsiraamat</Text>
            </Group>
            <Title order={1} ff={raamatuKiri} fw={600}>Funktsiooni uurimine</Title>
            <Text fz={19} lh={1.6} c="dimmed" ff={raamatuKiri}>
              Kuidas leida funktsiooni määramispiirkond, nullkohad, tuletis, ekstreemumid, kasvamis- ja
              kahanemisvahemikud ning käänupunktid – ja kuidas see rakendus neid arvutab.
            </Text>
            <Divider mt="sm" />
          </Stack>

          {/* ------------------------------------------------------------ 1 */}
          <Peatykk id="funktsioon" nr={1} pealkiri="Funktsioon">
            <Kast liik="definitsioon">
              <L>{String.raw`**Funktsioon** on eeskiri $f$, mis seab igale arvule $x$ hulgast $X$ vastavusse täpselt ühe arvu $y = f(x)$. Arvu $x$ nimetatakse **argumendiks**, arvu $y$ **funktsiooni väärtuseks**.`}</L>
            </Kast>
            <L>{String.raw`Funktsiooni saab anda valemiga (nt $f(x) = x^{3} - 3x$), tabeliga või graafikuga. Funktsiooni **graafik** on kõigi punktide $(x;\ f(x))$ hulk koordinaattasandil. Funktsiooni uurimise eesmärk on kirjeldada graafiku kuju ilma, et peaksime arvutama tuhandeid punkte: kus graafik lõikab x-telge, kus see tõuseb või langeb, kus on selle tipud ja kuidas see kõverdub.`}</L>
            <L>{String.raw`Väärtuse arvutamiseks asendame valemis $x$ antud arvuga. Näiteks kui $f(x) = x^{3} - 3x$, siis`}</L>
            <V>{'f(2) = 2^{3} - 3 \\cdot 2 = 8 - 6 = 2.'}</V>
          </Peatykk>

          {/* ------------------------------------------------------------ 2 */}
          <Peatykk id="maaramispiirkond" nr={2} pealkiri="Määramispiirkond">
            <Kast liik="definitsioon">
              <L>{String.raw`**Määramispiirkond** $X$ on kõigi nende argumendi väärtuste hulk, mille korral funktsiooni valem on arvutatav (annab reaalarvulise tulemuse).`}</L>
            </Kast>
            <L>{String.raw`Määramispiirkonda kitsendavad ainult mõned tehted. Iga sellise tehte kohta kirjutame tingimuse:`}</L>
            <Table withTableBorder verticalSpacing="sm" fz="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Avaldises esineb</Table.Th>
                  <Table.Th>Tingimus</Table.Th>
                  <Table.Th>Põhjus</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {[
                  ['\\frac{u}{v}', 'v \\neq 0', 'nulliga jagada ei saa'],
                  ['\\sqrt{u},\\ \\sqrt[4]{u}, \\dots', 'u \\geq 0', 'paarisjuur negatiivsest arvust puudub'],
                  ['\\ln u,\\ \\log_a u', 'u > 0', 'logaritmida saab ainult positiivset arvu'],
                  ['\\tan u', '\\cos u \\neq 0', '\\tan u = \\frac{\\sin u}{\\cos u}'],
                  ['\\cot u', '\\sin u \\neq 0', '\\cot u = \\frac{\\cos u}{\\sin u}'],
                  ['\\arcsin u,\\ \\arccos u', '-1 \\leq u \\leq 1', 'siinus ja koosinus on lõigus [-1; 1]'],
                ].map(([a, t, p]) => (
                  <Table.Tr key={a}>
                    <Table.Td><Valem latex={a} /></Table.Td>
                    <Table.Td><Valem latex={t} /></Table.Td>
                    <Table.Td>{p.includes('\\') ? <Valem latex={p} /> : p}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
            <L>{String.raw`Paaritu astme juur ($\sqrt[3]{x}$) on määratud kõigil reaalarvudel. Kui tingimusi on mitu, on määramispiirkond nende **ühisosa**. Kui ühtegi kitsendust ei ole (nt polünoomil), on $X = \mathbb{R}$.`}</L>
            <Kast liik="naide">
              <L>{String.raw`Leiame funktsiooni $f(x) = \dfrac{\sqrt{4 - x^{2}}}{x - 1}$ määramispiirkonna.`}</L>
              <V>{'\\begin{cases} 4 - x^{2} \\geq 0 \\\\ x - 1 \\neq 0 \\end{cases} \\Rightarrow \\begin{cases} -2 \\leq x \\leq 2 \\\\ x \\neq 1 \\end{cases} \\Rightarrow X = [-2;\\ 1) \\cup (1;\\ 2]'}</V>
              <Proovi valem="sqrt(4 - x^2)/(x - 1)" algus={-3} lopp={3} />
            </Kast>
          </Peatykk>

          {/* ------------------------------------------------------------ 3 */}
          <Peatykk id="nullkohad" nr={3} pealkiri="Nullkohad">
            <Kast liik="definitsioon">
              <L>{String.raw`**Nullkoht** on argumendi väärtus, mille korral funktsiooni väärtus on null: $f(x_0) = 0$. Graafikul on see punkt, kus graafik lõikab x-telge (või puudutab seda).`}</L>
            </Kast>
            <L>{String.raw`Nullkohtade leidmiseks lahendame võrrandi $f(x) = 0$. Sagedasemad võtted:`}</L>
            <List spacing="xs" ff={raamatuKiri} fz={17}>
              <List.Item><RikasTekst tekst="**Lineaarvõrrand** $ax + b = 0 \Rightarrow x = -\frac{b}{a}$." /></List.Item>
              <List.Item>
                <RikasTekst tekst="**Ruutvõrrand** $ax^{2} + bx + c = 0$ – diskriminandiga $D = b^{2} - 4ac$: kui $D > 0$, on kaks lahendit, kui $D = 0$, üks, kui $D < 0$, reaalarvulisi lahendeid pole." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Tegurdamine** – korrutis on null parajasti siis, kui vähemalt üks tegur on null: $x^{3} - 3x = x(x^{2} - 3) = 0$." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Murd** on null, kui lugeja on null ja nimetaja ei ole null." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Kõrgema astme polünoom** – ratsionaalsete juurte teoreem: täisarvuliste kordajate korral on ratsionaalne juur kujul $\frac{p}{q}$, kus $p$ jagab vabaliiget ja $q$ kõrgeima astme kordajat. Leitud juure $r$ abil jagame polünoomi tehtega $(x - r)$ (Horneri skeem) ja jätkame madalama astmega." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Eksponent- ja logaritmfunktsioon**: $e^{u} > 0$ alati, $\ln u = 0 \iff u = 1$." />
              </List.Item>
            </List>
            <V>{'x^{2} - 4x + 3 = 0:\\quad D = 16 - 12 = 4,\\quad x_{1,2} = \\frac{4 \\pm 2}{2} \\Rightarrow x_1 = 1,\\ x_2 = 3'}</V>
            <Kast liik="tahelepanu">
              <L>{String.raw`Leitud lahendid peavad kuuluma määramispiirkonda. Näiteks $x^{2} \ln x = 0$ annab $x = 0$ või $x = 1$, kuid $x = 0 \notin X$, seega on ainus nullkoht $x = 1$.`}</L>
            </Kast>
          </Peatykk>

          {/* ------------------------------------------------------------ 4 */}
          <Peatykk id="margid" nr={4} pealkiri="Positiivsus- ja negatiivsuspiirkond">
            <Kast liik="definitsioon">
              <L>{String.raw`**Positiivsuspiirkond** $X^{+}$ on argumendi väärtuste hulk, kus $f(x) > 0$ (graafik on x-teljest kõrgemal); **negatiivsuspiirkond** $X^{-}$ – kus $f(x) < 0$.`}</L>
            </Kast>
            <L>{String.raw`Pidev funktsioon saab märki muuta ainult nullkohtades ja kohtades, kus ta ei ole määratud. Need punktid jagavad arvtelje vahemikeks, kus märk on muutumatu. Seepärast piisab igas vahemikus ühe **testpunkti** väärtuse arvutamisest – see on **intervallmeetod**.`}</L>
            <Kast liik="naide" pealkiri="f(x) = x² − 4">
              <L>{String.raw`Nullkohad $x = \pm 2$ jagavad arvtelje kolmeks vahemikuks. Testpunktid annavad märgid:`}</L>
              <MargitabelVaade tabel={MARGI_NAIDE} />
              <V>{'X^{+} = (-\\infty;\\ -2) \\cup (2;\\ \\infty), \\qquad X^{-} = (-2;\\ 2)'}</V>
            </Kast>
          </Peatykk>

          {/* ------------------------------------------------------------ 5 */}
          <Peatykk id="tuletis" nr={5} pealkiri="Tuletis">
            <Kast liik="definitsioon">
              <L>{String.raw`Funktsiooni $f$ **tuletis** punktis $x$ on funktsiooni muudu ja argumendi muudu suhte piirväärtus:`}</L>
              <V>{"f'(x) = \\lim_{\\Delta x \\to 0} \\frac{\\Delta y}{\\Delta x} = \\lim_{\\Delta x \\to 0} \\frac{f(x + \\Delta x) - f(x)}{\\Delta x}"}</V>
            </Kast>
            <L>{String.raw`Tuletis näitab, **kui kiiresti funktsiooni väärtus muutub**. Geomeetriliselt on $f'(a)$ graafikule punktis $A(a;\ f(a))$ joonestatud puutuja tõus. Kui tuletis on positiivne, tõuseb puutuja (ja graafik) vasakult paremale; kui negatiivne, siis langeb.`}</L>
            <PuutujaJoonis />
            <Kast liik="naide" pealkiri="tuletis definitsiooni järgi">
              <L>{String.raw`Olgu $f(x) = x^{2}$. Siis`}</L>
              <V>{"f'(x) = \\lim_{\\Delta x \\to 0} \\frac{(x + \\Delta x)^{2} - x^{2}}{\\Delta x} = \\lim_{\\Delta x \\to 0} \\frac{2x\\,\\Delta x + (\\Delta x)^{2}}{\\Delta x} = \\lim_{\\Delta x \\to 0} (2x + \\Delta x) = 2x."}</V>
            </Kast>
            <L>{String.raw`Praktikas definitsiooni iga kord ei kasutata – tuletis leitakse **tuletiste tabeli** ja **diferentseerimisreeglite** abil (järgmine peatükk).`}</L>
          </Peatykk>

          {/* ------------------------------------------------------------ 6 */}
          <Peatykk id="tuletiste-tabel" nr={6} pealkiri="Tuletiste tabel ja reeglid">
            <Alapealkiri id="pohituletised">Põhifunktsioonide tuletised</Alapealkiri>
            <Table withTableBorder withColumnBorders striped verticalSpacing="xs" fz="md" maw={520}>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th w="50%"><Valem latex="f(x)" /></Table.Th>
                  <Table.Th><Valem latex="f'(x)" /></Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {TULETISED.map(([f, d]) => (
                  <Table.Tr key={f}>
                    <Table.Td><Valem latex={`\\displaystyle ${f}`} /></Table.Td>
                    <Table.Td><Valem latex={`\\displaystyle ${d}`} /></Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>

            <Alapealkiri id="reeglid">Diferentseerimisreeglid</Alapealkiri>
            <Table withTableBorder verticalSpacing="sm" fz="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Reegel</Table.Th>
                  <Table.Th>Valem</Table.Th>
                  <Table.Th>Näide</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {REEGLID.map(([nimi, valem, naide]) => (
                  <Table.Tr key={nimi}>
                    <Table.Td fw={500}>{nimi}</Table.Td>
                    <Table.Td><Valem latex={`\\displaystyle ${valem}`} /></Table.Td>
                    <Table.Td><Valem latex={`\\displaystyle ${naide}`} /></Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
            <Kast liik="naide" pealkiri="ahelreegel">
              <L>{String.raw`Funktsioon $f(x) = \sqrt{4 - x^{2}}$ on liitfunktsioon: välimine funktsioon on $\sqrt{u}$, sisemine $u = 4 - x^{2}$, $u' = -2x$. Seega`}</L>
              <V>{"f'(x) = \\frac{1}{2\\sqrt{4 - x^{2}}} \\cdot (-2x) = -\\frac{x}{\\sqrt{4 - x^{2}}}."}</V>
              <Proovi valem="sqrt(4 - x^2)" algus={-3} lopp={3} />
            </Kast>
            <L>{String.raw`**Teine tuletis** on tuletise tuletis: $f''(x) = (f'(x))'$. Näiteks $f(x) = x^{3} - 3x$ korral $f'(x) = 3x^{2} - 3$ ja $f''(x) = 6x$.`}</L>
          </Peatykk>

          {/* ------------------------------------------------------------ 7 */}
          <Peatykk id="ekstreemumid" nr={7} pealkiri="Monotoonsus ja ekstreemumid">
            <Kast liik="teoreem" pealkiri="monotoonsuse tunnus">
              <L>{String.raw`Kui vahemikus $f'(x) > 0$, siis funktsioon on selles vahemikus **kasvav**. Kui $f'(x) < 0$, siis **kahanev**.`}</L>
            </Kast>
            <Kast liik="definitsioon">
              <L>{String.raw`**Kriitiline punkt** on määramispiirkonna sisepunkt, kus $f'(x) = 0$ (**statsionaarne punkt**) või kus tuletist ei eksisteeri. Punkt $x_0$ on **maksimumpunkt**, kui selle ümbruses $f(x) \leq f(x_0)$, ja **miinimumpunkt**, kui $f(x) \geq f(x_0)$. Maksimume ja miinimume nimetatakse **ekstreemumiteks**.`}</L>
            </Kast>
            <Kast liik="teoreem" pealkiri="Fermat' teoreem (tarvilik tingimus)">
              <L>{String.raw`Kui diferentseeruval funktsioonil on punktis $x_0$ ekstreemum, siis $f'(x_0) = 0$.`}</L>
            </Kast>
            <Kast liik="teoreem" pealkiri="piisav tingimus">
              <L>{String.raw`Kui $f'$ muudab kriitilises punktis märki $+ \to -$, on seal **maksimum**; kui $- \to +$, siis **miinimum**. Kui märk ei muutu, ekstreemumit ei ole.`}</L>
            </Kast>
            <EkstreemumiJoonis />
            <Kast liik="tahelepanu">
              <L>{String.raw`$f'(x_0) = 0$ ei tähenda alati ekstreemumit: $f(x) = x^{3}$ korral $f'(0) = 0$, kuid tuletis on mõlemal pool positiivne ja funktsioon kasvab kogu arvteljel. Teisalt võib ekstreemum olla ka seal, kus tuletist pole: $f(x) = \sqrt[3]{x^{2}}$ miinimum on punktis $x = 0$.`}</L>
              <Group gap="xs">
                <Proovi valem="x^3" algus={-2} lopp={2} />
                <Proovi valem="x^(2/3)" algus={-3} lopp={3} />
              </Group>
            </Kast>
            <L>{String.raw`Teine võimalus ekstreemumi liigi määramiseks on **teise tuletise tunnus**: kui $f'(x_0) = 0$ ja $f''(x_0) < 0$, on $x_0$ maksimumpunkt; kui $f''(x_0) > 0$, siis miinimumpunkt.`}</L>
          </Peatykk>

          {/* ------------------------------------------------------------ 8 */}
          <Peatykk id="kumerus" nr={8} pealkiri="Kumerus, nõgusus ja käänupunktid">
            <Kast liik="teoreem">
              <L>{String.raw`Kui vahemikus $f''(x) > 0$, on graafik **nõgus** ($\cup$, „kauss“); kui $f''(x) < 0$, on graafik **kumer** ($\cap$, „küür“).`}</L>
            </Kast>
            <Kast liik="definitsioon">
              <L>{String.raw`**Käänupunkt** on graafiku punkt, kus kumerus vahetub nõgususega (või vastupidi). Selles punktis muudab teine tuletis märki – seega $f''(x) = 0$ või $f''$ ei eksisteeri.`}</L>
            </Kast>
            <KumerusJoonis />
            <L>{String.raw`Käänupunktide leidmine käib samamoodi nagu ekstreemumite leidmine, ainult $f'$ asemel uurime $f''$ märki: lahendame võrrandi $f''(x) = 0$, koostame märgitabeli ja kontrollime, kas märk muutub.`}</L>
          </Peatykk>

          {/* ------------------------------------------------------------ 9 */}
          <Peatykk id="asumptoodid" nr={9} pealkiri="Asümptoodid">
            <L>{String.raw`**Püstasümptoot** $x = a$ tekib, kui funktsiooni väärtus kasvab punktile $a$ lähenedes tõkestamatult: $\lim_{x \to a} |f(x)| = \infty$. Tavaliselt on see nimetaja nullkoht (nt $\frac{1}{x - 2}$ korral $x = 2$) või logaritmi argumendi nullkoht ($\ln x$ korral $x = 0$).`}</L>
            <L>{String.raw`**Rõhtasümptoot** $y = b$ tekib, kui $\lim_{x \to \pm\infty} f(x) = b$. Näiteks $\frac{x^{2} - 1}{x^{2} + 1} \to 1$, kui $x \to \pm\infty$.`}</L>
            <Proovi valem="1/(x - 2)" />
          </Peatykk>

          {/* ------------------------------------------------------------ 10 */}
          <Peatykk id="skeem" nr={10} pealkiri="Funktsiooni uurimise skeem">
            <L>{String.raw`Funktsiooni täielik uurimine käib tavaliselt järgmises järjekorras:`}</L>
            <List type="ordered" spacing="xs" ff={raamatuKiri} fz={17}>
              {[
                'Määramispiirkond $X$.',
                'Nullkohad: lahenda $f(x) = 0$.',
                'Positiivsus- ja negatiivsuspiirkond (intervallmeetod).',
                "Tuletis $f'(x)$.",
                "Kriitilised punktid: $f'(x) = 0$ või $f'$ puudub.",
                "Märgitabel: kasvamis- ja kahanemisvahemikud $X↑$, $X↓$.",
                'Ekstreemumid ja nende väärtused $f(x_0)$.',
                "Teine tuletis $f''(x)$, käänupunktid, kumerus- ja nõgususvahemikud.",
                'Asümptoodid ja graafiku skitseerimine.',
              ].map((s) => (
                <List.Item key={s}><RikasTekst tekst={s} /></List.Item>
              ))}
            </List>

            <Alapealkiri id="taisnaide">Täisnäide: f(x) = x³ − 3x</Alapealkiri>
            <Stack gap="sm">
              <L>{String.raw`**1. Määramispiirkond.** Polünoom on määratud kõikjal: $X = \mathbb{R}$.`}</L>
              <L>{String.raw`**2. Nullkohad.** $x^{3} - 3x = x(x^{2} - 3) = 0 \Rightarrow x = 0$ või $x = \pm\sqrt{3}$.`}</L>
              <L>{String.raw`**3. Tuletis.** Summa reegli ja astme tuletise abil $f'(x) = 3x^{2} - 3$.`}</L>
              <L>{String.raw`**4. Kriitilised punktid.** $3x^{2} - 3 = 0 \iff x^{2} = 1 \iff x = \pm 1$.`}</L>
              <L>{String.raw`**5. Märgitabel.** Testpunktid $-2$, $0$ ja $2$ annavad tuletise märgid:`}</L>
              <MargitabelVaade tabel={NAITE_TABEL} />
              <L>{String.raw`Seega $X↑ = (-\infty;\ -1) \cup (1;\ \infty)$ ja $X↓ = (-1;\ 1)$.`}</L>
              <L>{String.raw`**6. Ekstreemumid.** $y_{\max} = f(-1) = (-1)^{3} - 3 \cdot (-1) = 2$ ja $y_{\min} = f(1) = 1 - 3 = -2$.`}</L>
              <L>{String.raw`**7. Käänupunkt.** $f''(x) = 6x = 0 \iff x = 0$; $f''$ muudab seal märki $- \to +$, seega $K(0;\ 0)$; graafik on kumer vahemikus $(-\infty;\ 0)$ ja nõgus vahemikus $(0;\ \infty)$.`}</L>
              <Proovi valem="x^3 - 3x" algus={-3} lopp={3} />
            </Stack>
          </Peatykk>

          {/* ------------------------------------------------------------ 11 */}
          <Peatykk id="arvutamine" nr={11} pealkiri="Kuidas rakendus arvutab">
            <L>{String.raw`Rakendus ei joonista graafikut ega otsi vastuseid „silma järgi“ – kõik arvutab server **sümbolarvutuse** abil (teek MathNet.Symbolics), nii nagu sina paberil:`}</L>
            <List spacing="xs" ff={raamatuKiri} fz={17}>
              <List.Item>
                <RikasTekst tekst="**Valemi lugemine.** Tekst teisendatakse avaldispuuks; samal ajal kogutakse määramispiirkonna tingimused (nimetaja ≠ 0, juuritav ≥ 0, logaritmitav > 0) enne lihtsustamist, et nt $\frac{x}{x}$ korral ei kaoks tingimus $x \neq 0$." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Tuletised** leitakse diferentseerimisreeglitega sümboolselt ja lihtsustatakse; valitakse kõige lühem samaväärne kuju." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Võrrandid** lahendatakse samas järjekorras nagu käsitsi: tegurdamine, murru lugeja, logaritmi ja eksponendi omadused, polünoomid. Polünoomi kõik reaalarvulised juured leitakse tema **kaasmaatriksi omaväärtustena** ja täpsustatakse **Newtoni meetodiga**." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Mittealgebralised võrrandid** (nt $\sin x = 0$) lahendatakse **numbriliselt** valitud lõigul: lõik jagatakse 4000 osaks ja kus avaldis muudab märki, täpsustatakse juur **Brenti meetodiga**. Sellised vastused on märgitud „vahemikus [a; b]“." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Täpne kuju.** Numbriliselt leitud arvust tuntakse ära murd, ruutjuur, $\pi$ kordne või $e$ aste, nii et vastus on $x = -\sqrt{3} \approx -1.7321$, mitte ainult $-1.7321$." />
              </List.Item>
              <List.Item>
                <RikasTekst tekst="**Märgid** määratakse intervallmeetodiga: testpunkt igas vahemikus." />
              </List.Item>
            </List>
            <L>{String.raw`Iga vastuse lahenduskäiku saad vaadata lahendajas nupust **„Näita rohkem ja lahenduskäiku“**.`}</L>
            <Group>
              <Button component={Link} to="/uus" rightSection={<IconArrowRight size={16} />}>Ava lahendaja</Button>
            </Group>
          </Peatykk>
        </Stack>
      </Grid.Col>

      <Grid.Col span={{ base: 12, lg: 3 }} visibleFrom="lg">
        <Box pos="sticky" top={84}>
          <Group gap={6} mb="xs">
            <Badge variant="light" radius="sm">Sisukord</Badge>
          </Group>
          <TableOfContents
            variant="light"
            size="sm"
            radius="sm"
            minDepthToOffset={1}
            depthOffset={16}
            scrollSpyOptions={{
              selector: '[data-toc]',
              getDepth: (el) => (el.dataset.toc === 'peatykk' ? 1 : 2),
              getValue: (el) => el.textContent?.replace(/^\d+\.\s*/, '') ?? '',
              offset: 90,
            }}
            getControlProps={({ data }) => ({
              onClick: () => data.getNode().scrollIntoView({ behavior: 'smooth' }),
              children: data.value,
            })}
          />
          <Anchor component={Link} to="/uus" size="sm" mt="md" display="block">
            → Mine lahendajasse
          </Anchor>
        </Box>
      </Grid.Col>
    </Grid>
  )
}
