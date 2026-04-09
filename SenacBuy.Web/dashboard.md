# Gráfico de Barras do Dashboard

O gráfico de pedidos por mês (`ucDashboard.cs`) é desenhado **manualmente via GDI+**, sem bibliotecas externas, usando o evento `Paint` de um `Panel`.

---

## Como funciona

### 1. Associar o evento Paint ao painel

No construtor do UserControl, registre o método de desenho no evento `Paint` do painel onde o gráfico será exibido:

```csharp
panelFill.Paint += PanelFill_Paint;
```

Quando quiser forçar o redesenho (ex: após carregar novos dados), chame:

```csharp
panelFill.Invalidate();
```

---

### 2. Carregar os dados

Os dados vêm de `DashboardApiService.GetDashboardAsync()`, que retorna um `DashboardDto` contendo a lista `PedidosPorMes` — uma coleção de objetos com `Mes` (número 1–12) e `Total` (quantidade de pedidos).

```csharp
_dados = await _dashService.GetDashboardAsync();
panelFill.Invalidate(); // dispara o Paint com os dados novos
```

---

### 3. Desenhar o gráfico no evento Paint

O método `PanelFill_Paint` recebe um `Graphics` e usa as dimensões do painel para calcular proporções automaticamente.

```csharp
private void PanelFill_Paint(object? sender, PaintEventArgs e)
{
    var g     = e.Graphics;
    var dados = _dados.PedidosPorMes;
    int max   = dados.Max(d => d.Total); // valor máximo para normalizar alturas
```

#### Parâmetros de layout

| Variável  | Valor | Descrição                              |
|-----------|-------|----------------------------------------|
| `barW`    | 40    | Largura de cada barra em pixels        |
| `barGap`  | 30    | Espaçamento entre barras               |
| `startX`  | 50    | Margem esquerda                        |
| `baseY`   | `Height - 50` | Linha de base (eixo X)        |
| `maxH`    | `Height - 100` | Altura máxima disponível para barras |

#### Cálculo da altura de cada barra

A altura é **proporcional ao valor máximo**, para que a barra com mais pedidos ocupe todo o espaço disponível:

```csharp
int altura = (int)((double)dados[i].Total / max * maxH);
int y      = baseY - altura; // coordenada Y de onde a barra começa
```

#### Desenhando cada elemento

Para cada mês na lista:

```csharp
// 1. Posição X da barra
int x = startX + i * (barW + barGap);

// 2. Barra azul
g.FillRectangle(brushBarra, x, y, barW, altura);

// 3. Valor numérico acima da barra
g.DrawString(dados[i].Total.ToString(), font, brushText, new PointF(x + 2, y - 16));

// 4. Rótulo do mês abaixo da barra
string mesLabel = meses[dados[i].Mes - 1]; // converte número para "Jan", "Fev"...
g.DrawString(mesLabel, font, brushText, new PointF(x + 5, baseY + 5));
```

---

## Estilo visual

| Elemento        | Cor / Fonte                        |
|-----------------|------------------------------------|
| Barra           | `Color.FromArgb(19, 127, 236)` (azul) |
| Texto e rótulos | `Color.FromArgb(60, 70, 90)` (cinza escuro) |
| Fonte rótulos   | `Segoe UI, 8pt`                    |
| Fonte título    | `Segoe UI, 10pt, Bold`             |

---

## Fluxo completo resumido

```
UserControl.Load
    └── CarregarDashboardAsync()
            └── DashboardApiService.GetDashboardAsync()  →  _dados
            └── Atualiza labels KPI
            └── panelFill.Invalidate()
                    └── PanelFill_Paint()
                            └── Desenha título + barras + valores + rótulos
```

---

## Pontos de atenção

- Se `_dados` for `null` ou `PedidosPorMes` estiver vazio, o método retorna sem desenhar nada.
- Se o valor máximo for `0` (sem pedidos), o método também retorna para evitar divisão por zero.
- O número de meses exibidos depende do tamanho da lista retornada pela API (configurado para os **últimos 6 meses**).
- Para mais meses, reduza `barW` e `barGap` ou aumente a largura do `panelFill`.
