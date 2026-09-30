import { useComputedColorScheme, useMantineTheme } from '@mantine/core'
import { LineChart, ScatterChart } from 'echarts/charts'
import {
  DataZoomComponent,
  GridComponent,
  LegendComponent,
  MarkLineComponent,
  ToolboxComponent,
  TooltipComponent,
} from 'echarts/components'
import * as echarts from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { useEffect, useRef } from 'react'
import type { GraafikuVastus } from '../api/tyybid'

// ainult vajalikud ECharts moodulid (väiksem kimp); joonestamine käib <canvas> elemendile
echarts.use([
  LineChart,
  ScatterChart,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  DataZoomComponent,
  MarkLineComponent,
  ToolboxComponent,
  CanvasRenderer,
])

interface Props {
  andmed: GraafikuVastus
  algus: number
  lopp: number
  laadib?: boolean
  korgus?: number
}

const vorminda = (v: number) => (Math.abs(v) < 1e-10 ? '0' : Number(v.toFixed(4)).toString())

/**
 * y-telje ulatus. Püstasümptootide korral lõigatakse ära äärmised väärtused (3%/97% kvantiil),
 * muidu mahub kogu graafik.
 */
function yUlatus(seeriad: (number | null)[][], asumptoote: boolean) {
  const v = seeriad.flat().filter((y): y is number => y !== null && Number.isFinite(y)).sort((a, b) => a - b)
  if (v.length === 0) return { min: -1, max: 1 }
  const kvantiil = (p: number) => v[Math.round(p * (v.length - 1))]
  let [alumine, ulemine] = asumptoote ? [kvantiil(0.03), kvantiil(0.97)] : [v[0], v[v.length - 1]]
  if (ulemine - alumine < 1e-9) {
    alumine -= 1
    ulemine += 1
  }
  const varu = (ulemine - alumine) * 0.08
  const samm = 10 ** Math.floor(Math.log10(ulemine - alumine)) / 2
  return {
    min: Math.floor((alumine - varu) / samm) * samm,
    max: Math.ceil((ulemine + varu) / samm) * samm,
  }
}

export function FunktsiooniGraafik({ andmed, algus, lopp, laadib = false, korgus = 460 }: Props) {
  const konteiner = useRef<HTMLDivElement>(null)
  const graafik = useRef<echarts.ECharts | null>(null)
  const teema = useMantineTheme()
  const tume = useComputedColorScheme('light') === 'dark'

  useEffect(() => {
    const g = echarts.init(konteiner.current!, undefined, { renderer: 'canvas' })
    graafik.current = g
    const jalgija = new ResizeObserver(() => g.resize())
    jalgija.observe(konteiner.current!)
    return () => {
      jalgija.disconnect()
      g.dispose()
      graafik.current = null
    }
  }, [])

  useEffect(() => {
    const g = graafik.current
    if (!g) return
    const c = teema.colors
    const varvid = {
      f: tume ? c.indigo[4] : c.indigo[7],
      tuletis: tume ? c.red[4] : c.red[7],
      teine: tume ? c.teal[4] : c.teal[7],
      null: tume ? c.gray[1] : c.dark[6],
      ekstreemum: c.orange[6],
      kaanupunkt: c.grape[6],
      tekst: tume ? c.dark[1] : c.gray[7],
      telg: tume ? c.dark[2] : c.gray[8],
      vork: tume ? c.dark[5] : c.gray[2],
      asumptoot: tume ? c.dark[3] : c.gray[5],
      taust: tume ? c.dark[7] : '#fff',
    }
    const paarid = (y: (number | null)[]) => andmed.x.map((x, i) => [x, y[i]])
    // asümptootide korral skaleerime ainult f(x) järgi – tuletise poolused on veel järsemad
    const asumptoote = andmed.asumptoodid.length > 0
    const { min, max } = yUlatus(asumptoote ? [andmed.y] : [andmed.y, andmed.yTuletis], asumptoote)
    const telg = {
      type: 'value' as const,
      axisLine: { onZero: true, symbol: ['none', 'arrow'], symbolSize: [7, 10], lineStyle: { color: varvid.telg } },
      axisLabel: { color: varvid.tekst },
      splitLine: { lineStyle: { color: varvid.vork } },
      nameTextStyle: { color: varvid.tekst, fontStyle: 'italic' as const, fontSize: 14 },
    }

    g.setOption(
      {
        backgroundColor: 'transparent',
        animationDuration: 300,
        textStyle: { fontFamily: teema.fontFamily },
        grid: { left: 48, right: 28, top: 64, bottom: 36 },
        legend: {
          type: 'scroll',
          top: 4,
          left: 0,
          right: 0,
          pageTextStyle: { color: varvid.tekst },
          textStyle: { color: varvid.tekst },
          selected: { "f''(x)": false },
        },
        tooltip: {
          trigger: 'axis',
          axisPointer: { type: 'line', lineStyle: { color: varvid.asumptoot } },
          backgroundColor: varvid.taust,
          textStyle: { color: varvid.tekst },
          formatter: (parameetrid: unknown) => {
            const read = (parameetrid as { seriesType: string; seriesName: string; marker: string; value: [number, number | null] }[])
              .filter((p) => p.seriesType === 'line' && p.value[1] !== null)
            if (read.length === 0) return ''
            return [`x = ${vorminda(read[0].value[0])}`, ...read.map((p) => `${p.marker}${p.seriesName} = ${vorminda(p.value[1]!)}`)].join('<br/>')
          },
        },
        toolbox: {
          right: 8,
          top: 28, // legendi all, et kitsal ekraanil ei kattuks
          itemSize: 14,
          iconStyle: { borderColor: varvid.tekst },
          feature: {
            dataZoom: { title: { zoom: 'Suurenda ala', back: 'Tagasi' }, filterMode: 'none' },
            restore: { title: 'Taasta' },
            saveAsImage: { title: 'Salvesta pildina', name: 'funktsioon', backgroundColor: varvid.taust },
          },
        },
        dataZoom: [
          { type: 'inside', xAxisIndex: 0, filterMode: 'none' },
          { type: 'inside', yAxisIndex: 0, filterMode: 'none' },
        ],
        xAxis: { ...telg, name: 'x', min: algus, max: lopp },
        yAxis: { ...telg, name: 'y', min, max },
        series: [
          {
            name: 'f(x)',
            type: 'line',
            data: paarid(andmed.y),
            showSymbol: false,
            clip: true,
            connectNulls: false,
            lineStyle: { width: 2.5, color: varvid.f },
            itemStyle: { color: varvid.f },
            z: 3,
            markLine: {
              silent: true,
              symbol: 'none',
              lineStyle: { type: 'dashed', color: varvid.asumptoot, width: 1.2 },
              label: { formatter: 'x = {c}', color: varvid.tekst },
              data: andmed.asumptoodid.map((a) => ({ xAxis: a })),
            },
          },
          {
            name: "f'(x)",
            type: 'line',
            data: paarid(andmed.yTuletis),
            showSymbol: false,
            clip: true,
            connectNulls: false,
            lineStyle: { width: 2, type: [8, 6], color: varvid.tuletis },
            itemStyle: { color: varvid.tuletis },
            z: 2,
          },
          {
            name: "f''(x)",
            type: 'line',
            data: paarid(andmed.yTeineTuletis),
            showSymbol: false,
            clip: true,
            connectNulls: false,
            lineStyle: { width: 1.5, type: [2, 4], color: varvid.teine },
            itemStyle: { color: varvid.teine },
            z: 1,
          },
          {
            name: 'Nullkohad',
            type: 'scatter',
            data: andmed.nullkohad.map((p) => [p.x, p.y]),
            symbolSize: 9,
            itemStyle: { color: varvid.taust, borderColor: varvid.null, borderWidth: 2 },
            z: 4,
          },
          {
            name: 'Ekstreemumid',
            type: 'scatter',
            data: andmed.ekstreemumid.map((e) => ({
              value: [e.x, e.y],
              name: e.tyyp,
              symbolRotate: e.tyyp === 'max' ? 0 : 180,
              label: { position: e.tyyp === 'max' ? 'top' : 'bottom' },
            })),
            symbol: 'triangle',
            symbolSize: 12,
            label: { show: true, formatter: '{b}', color: varvid.ekstreemum, fontWeight: 'bold' },
            itemStyle: { color: varvid.ekstreemum },
            z: 5,
          },
          {
            name: 'Käänupunktid',
            type: 'scatter',
            data: andmed.kaanupunktid.map((p) => [p.x, p.y]),
            symbol: 'diamond',
            symbolSize: 11,
            itemStyle: { color: varvid.kaanupunkt },
            z: 4,
          },
        ],
      },
      { notMerge: true },
    )
    // ülesande nõue: joonis on <canvas id="funktsiooniGraafik"> (ECharts loob lõuendi esimesel joonistamisel)
    g.getDom().querySelector('canvas')?.setAttribute('id', 'funktsiooniGraafik')
  }, [andmed, algus, lopp, tume, teema])

  useEffect(() => {
    const g = graafik.current
    if (!g) return
    if (laadib) g.showLoading({ text: '', color: teema.colors.indigo[6], maskColor: 'rgba(128,128,128,0.08)' })
    else g.hideLoading()
  }, [laadib, teema])

  return (
    <div
      ref={konteiner}
      role="img"
      aria-label={`Funktsiooni ja tuletise graafik vahemikus [${algus}; ${lopp}]`}
      style={{ width: '100%', height: korgus }}
    />
  )
}
