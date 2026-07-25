# Módulo Sanitário & Período de Carência (`Modules.Sanitary`)

## 1. Visão Geral
O módulo `Modules.Sanitary` gerencia o calendário oficial de vacinações (Febre Aftosa, Brucelose, Raiva, Clostridioses), controle veterinário de aplicação de medicamentos/vermífugos e calcula com precisão matemática a **data final de carência sanitária**.

---

## 2. Bloqueio Rígido de Abate / Carência Sanitária
Para garantir conformidade com as normas do MAPA (Ministério da Agricultura e Pecuária) e órgãos de defesa sanitária animal:
- Sempre que um animal ou lote recebe uma medicação ou vacina com `WithdrawalDays > 0`, o sistema registra a data de liberação `WithdrawalEndDateUtc = ApplicationDateUtc.AddDays(WithdrawalDays)`.
- Se for feita uma consulta de elegibilidade para abate (`ValidateSlaughterEligibilityQuery`), e a data atual for anterior à data de liberação de carência, o backend retorna `Result.Failure(SanitaryErrors.ActiveSlaughterWithdrawalPeriod)`.
- No Frontend Blazor, a interface renderiza a badge `<SlaughterWithdrawalBadge>` em vermelho, indicando que o abate do animal/lote está totalmente bloqueado.

---

## 3. Entidades Principais
- **`VaccinationCampaign`**: Registra campanhas oficiais com período de vigência (`StartDateUtc` a `EndDateUtc`), tipo da campanha e status de atividade.
- **`TreatmentRecord`**: Registra a aplicação individual ou em lote de produto comercial, dosagem, número do lote, veterinário responsável e dias de carência.
- **`WithdrawalPeriodService`**: Serviço de domínio responsável por avaliar a inelegibilidade de abate.

---

## 4. API Endpoints
- `GET /api/sanitary/campaigns` - Lista campanhas sanitárias ativas.
- `POST /api/sanitary/campaigns` - Cria nova campanha oficial de vacinação.
- `POST /api/sanitary/treatments` - Registra aplicação de medicamento/vacina com dias de carência.
- `GET /api/sanitary/slaughter-validation/{animalId}` - Valida se o animal está liberado para abate ou se possui carência sanitária ativa.

---

## 5. Garantia TDD & Testes
- Testes unitários do domínio sanitário localizados em `tests/Unit/CriaCerto.Modules.Sanitary.UnitTests`.
