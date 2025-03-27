# Postmortem - Match-3 Game

## Histórico de Problemas e Soluções no Desenvolvimento do Match-3

Durante o desenvolvimento do projeto do jogo **Match-3**, diversos desafios técnicos surgiram ao longo da implementação do sistema principal de grid, movimentação e mecânicas de jogo.
Este relatório tem como objetivo registrar alguns desses problemas encontrados, as soluções aplicadas e o raciocínio técnico utilizado para resolver cada um deles, servindo tanto como documentação de processo quanto como material de aprendizado e análise futura.

---

### Problema: Movimentação de peças para fora do grid

**Descrição:**  
Inicialmente, os doces estavam saindo do grid, movendo-se para locais inválidos e era possível arrastá-los para qualquer direção e distância.

**Solução:**  
- Corrigido o sistema de swap para aceitar apenas movimentações dentro dos limites do grid.
- Implementado bloqueio de movimentação para apenas uma casa adjacente por jogada.
- Adicionada pintura visual para destacar os matches durante testes, até ser implementada a mecânica final de destruição dos matches.
- Realizada uma bateria de testes para garantir que ajustes pontuais não afetassem outras funcionalidades.

---

### Problema: Movimentação sem formação de match

**Descrição:**  
Qualquer movimentação era aceita, mesmo sem gerar um match válido, quebrando a proposta do gameplay.

**Solução:**  
Implementada verificação pós-swap: ao realizar um movimento, o sistema verifica imediatamente se há match; caso não haja, o movimento não é feito.

---

### Problema: Queda de peças lenta e travada

**Descrição:**  
Após um match, apenas uma peça caía por vez, travando o fluxo do jogo.

**Solução:**  
Implementadas **coroutines** para permitir movimentação assíncrona de várias peças ao mesmo tempo, garantindo fluidez.

---

### Problema: Falta de spawn de novas peças

**Descrição:**  
Após matches, novas peças não eram geradas corretamente para preencher o grid.

**Solução:**  
Implementado método de spawn controlado no `UpdateGridAfterMatch()`, sincronizando a criação de novos doces com a finalização da movimentação anterior.

---

### Problema: Sobreposição visual de peças

**Descrição:**  
Novas peças recém-spawnadas apareciam sobre doces matched que ainda não haviam sido destruídos.

**Solução:**  
Como solução temporária, para ajustar o projeto dentro do escopo proposto e do prazo determinado, as peças matched passaram a ficar invisíveis assim que identificadas como parte de um combo, evitando sobreposição visual.

---

### Problema: Matches não destruídos automaticamente

**Descrição:**  
Matches existentes no grid só eram destruídos após uma jogada ativa do jogador.

**Solução:**  
Criada função `ClearMatches()`, responsável por limpar automaticamente qualquer combinação existente após cada movimentação.

---

### Problema: Falta de movimentos válidos no grid

**Descrição:**  
O grid poderia gerar situações sem movimentos válidos, bloqueando a progressão.

**Solução:**  
Implementado sistema de **reshuffle automático**, detectando essa condição e embaralhando o grid para garantir jogabilidade contínua.

---

### Problema: Falta de feedback visual para o jogador

**Descrição:**  
Ausência de indicações visuais ao jogador durante a ociosidade ou ao realizar matches.

**Solução:**  
- Doces passam a pulsar quando o jogador fica ocioso.
- Partículas visuais adicionadas ao destruir doces.
- Pequenas animações para jogadas inválidas e quedas de peças.

---

### Regras e ajustes adicionais

- Limitação de movimentos por fase  
- Definição do tamanho do grid  
- Limitação de input durante cascatas, quedas ou ativações de power-ups  
- Botões de debug implementados para facilitar testes

---

### Possíveis melhorias futuras

- Implementação de **Object Pooling** para otimização de performance.
- Garantir que o grid não inicie com matches automáticos, para evitar acúmulo de pontos sem ação do jogador.

---
