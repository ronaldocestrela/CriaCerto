# Módulo Analytics & Exportação Executiva (CriaCerto)

## Visão Geral
O módulo `Analytics` é responsável pela consolidação zootécnica multi-fazenda e pela geração/exportação de relatórios executivos e regulatórios para suporte ao manejo do rebanho bovino (corte e leite) e à emissão da **GTA (Guia de Trânsito Animal)**.

---

## 1. Tipos de Relatórios Suportados

### 1.1 Scorecard Executivo Zootécnico
- **Taxa de Prenhez (%):** `(Vacas Prenhes / Total de Vacas Matrizes) * 100`
- **Taxa de Desmame (%):** `(Bezerros Desmamados / Total de Vacas Matrizes) * 100`
- **Taxa de Lotação (UA/ha):** `Total de Unidades Animais (UA) / Área Total de Pastagens (ha)`
- **Ganho de Peso Diário (GPD kg):** Média ponderada de ganho de peso diário do rebanho por lote
- **Custo por Arroba Produzida (R$/@):** Custo nutricional e operacional por arroba (@)
- **Status de Saúde Sanitária:** Alerta reativo baseado na contagem de animais em período de carência sanitária medicamentosa (`AnimalsUnderSlaughterWithdrawal`)

### 1.2 Inventário Completo do Rebanho
- Consolidação quantitativa do plantel categorizada por:
  - Matrizes / Vacas
  - Bezerros (Desmamados)
  - Novilhos / Garrotes (Recria)
  - Bois Gordos (Terminação / Engorda)
  - Touros Reprodutores
- Métricas consolidadas: Quantidade de cabeças, peso total em kg, total de arrobas (@) e peso médio por categoria.

### 1.3 Suporte à Emissão de GTA (Guia de Trânsito Animal)
- Relatório de conformidade oficial com órgãos estaduais e federais de defesa sanitária (IAGRO, INDEA, MAPA, etc.).
- Detalhamento por faixa etária oficial e sexo:
  - 0 a 12 meses (Machos / Fêmeas)
  - 13 a 24 meses (Machos / Fêmeas)
  - 25 a 36 meses (Machos / Fêmeas)
  - Acima de 36 meses (Machos / Fêmeas)
- Trava Sanitária: Declaração de carência sanitária zero e status de vacinações obrigatórias (Febre Aftosa e Brucelose).

---

## 2. Formatos de Exportação Suportados

1. **CSV (`text/csv`):** Arquivo de texto separado por vírgula codificado em UTF-8 com suporte universal para planilhas e scripts.
2. **Excel (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`):** Arquivo XML Spreadsheet otimizado com cabeçalhos estilizados e tabelas de dados.
3. **PDF (`application/pdf`):** Relatório em formato de documento oficial estilizado com cabeçalho zootécnico, carimbo de data/hora e marca d'água de integridade sanitária.

---

## 3. Endpoints da API

- `GET /api/analytics/executive-scorecard`: Retorna o DTO `ExecutiveScorecardDto` com os KPIs zootécnicos.
- `POST /api/analytics/export`: Aceita a query `ExportBovineReportQuery` (tipo de relatório, formato, filtro de período `CurrentHarvest`, `OffSeason`, `CurrentMonth`, `CustomRange`) e retorna o download do arquivo via stream binário com o header `Content-Disposition`.
- `POST /api/analytics/export-csv`: Endpoint legado de compatibilidade para exportação simples em CSV.
