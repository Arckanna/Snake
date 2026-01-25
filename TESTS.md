# Tests unitaires — Guide

Ce document explique **à quoi servent les tests unitaires**, **pourquoi** on les écrit, **ce qu’on teste** dans ce projet et **comment** les utiliser.

---

## 1. À quoi servent les tests unitaires ?

Un **test unitaire** est un petit programme qui :

1. **Prépare** un état (créer un moteur, initialiser une grille, etc.)
2. **Exécute** une action (par ex. `Move(Direction.Right)`)
3. **Vérifie** que le résultat est celui attendu (nouvelle position, `GameOver`, etc.)

Ils servent à :

- **Valider le comportement** : s’assurer que la logique (moteur de jeu, calculs, règles) fait bien ce qu’on a en tête.
- **Détecter les régressions** : quand on modifie le code, les tests indiquent tout de suite si un comportement a « cassé ».
- **Documenter** : les noms des tests et leurs scénarios décrivent comment le système est censé se comporter.
- **Donner de la confiance** : on peut refactorer ou ajouter des fonctionnalités en s’appuyant sur une base de tests qui tourne à chaque fois.

---

## 2. Pourquoi les utiliser ?

| Sans tests | Avec tests |
|-----------|------------|
| On modifie le code, on lance le jeu à la main, on espère ne rien avoir oublié. | On modifie, on lance `dotnet test` : en quelques secondes on sait si la logique tient la route. |
| Un bug revient plus tard sans qu’on s’en rende compte. | Un test existant échoue et signale le bug avant qu’il ne parte en prod. |
| On hésite à toucher au code par peur de casser quelque chose. | On ose refactorer : les tests vérifient que le comportement reste le même. |

En résumé : les tests rendent les changements **plus sûrs** et **plus rapides** à valider.

---

## 3. Ce qu’on teste ici : le `GameEngine`

On ne teste **pas** l’interface (fenêtres WPF, boutons, dessin). On teste la **logique de jeu** dans `GameEngine` (dans `Snake.Core`), car :

- Elle est **séparée** de WPF : pas de fenêtres, pas de timer Windows.
- Elle contient les **règles** : déplacement, collisions, demi-tours, nourriture.

### 3.1 Initialisation (`Initialize`)

| Test | Ce qu’on vérifie | Pourquoi c’est important |
|------|------------------|---------------------------|
| `Initialize_QuandAppele_StateEstPlaying` | `State == Playing` | La partie démarre bien en mode jeu. |
| `Initialize_QuandAppele_ScoreEstZero` | `Score == 0` | Le score repart de 0 à chaque nouvelle partie. |
| `Initialize_QuandAppele_SerpentALongueurDemandee` | `SnakeParts.Count == 7` pour une longueur 7 | La taille initiale du serpent est respectée. |
| `Initialize_QuandAppele_SquareSizeEstEnregistre` | `SquareSize == 20` | La taille des cases est bien stockée (déplacements, dessin). |
| `Initialize_QuandAppele_TeteAuBoutDroite` | Tête en `(60, 40)` pour 4 segments, carré 20 | La position initiale et l’ordre des segments sont cohérents. |
| `Initialize_QuandGrilleAssezGrande_UnFruitEstPlace` | `FoodPosition` non null, dans la grille | Un fruit est bien généré au début. |

### 3.2 Déplacements (`Move`)

| Test | Ce qu’on vérifie | Pourquoi c’est important |
|------|------------------|---------------------------|
| `Move_DirectionDroite_TeteSeDeplaceADroite` | La tête avance de `SquareSize` en X, Y inchangé | Le déplacement à droite fonctionne. |
| `Move_DirectionGauche_TeteSeDeplaceAGauche` | Après Up puis Left, la tête recule de `SquareSize` en X | Le déplacement à gauche fonctionne. |
| `Move_DemiTourGaucheQuandDroite_ConserveDirectionDroite` | Un `Move(Left)` alors qu’on va à droite est **ignoré**, la tête continue à droite | Les demi-tours sont interdits (sinon le serpent se mordrait « tout de suite »). |

### 3.3 Collisions et fin de partie

| Test | Ce qu’on vérifie | Pourquoi c’est important |
|------|------------------|---------------------------|
| `Move_TeteEntreDansMur_StatePasseAGameOver` | En allant dans le mur, `State == GameOver` | La collision avec les bords arrête la partie. |
| `Move_TeteEntreDansSonCorps_StatePasseAGameOver` | En revenant sur son corps, `State == GameOver` | La morsure de queue arrête la partie. |
| `Move_QuandDejaGameOver_NeChangePlusRien` | Après un Game Over, d’autres `Move` ne changent plus l’état | On ne peut plus jouer une fois la partie terminée. |

---

## 4. Comment les lancer ?

À la racine du dépôt (ou depuis le dossier du projet) :

```bash
# Tous les tests du projet Snake.Tests
dotnet test Snake.Tests/Snake.Tests.csproj
```

Ou en passant par la solution :

```bash
dotnet test Snake.sln
```

### Exécuter certains tests seulement

```bash
# Par nom de méthode
dotnet test Snake.Tests/Snake.Tests.csproj --filter "FullyQualifiedName~Move_DirectionDroite"

# Par classe
dotnet test Snake.Tests/Snake.Tests.csproj --filter "FullyQualifiedName~GameEngineTests"
```

### Sortie typique

```
Réussi!  - échec : 0, réussite : 12, ignorée(s) : 0, total : 12
```

Si un test échoue, la sortie indique le test en cause, la valeur attendue et la valeur obtenue, ce qui permet de cibler le correctif.

---

## 5. Bonnes pratiques utilisées ici

1. **Un cas par test** : chaque test vérifie une idée précise (ex. « demi-tour ignoré »).
2. **Noms explicites** : `Move_TeteEntreDansMur_StatePasseAGameOver` décrit le scénario et le résultat.
3. **Pas de WPF dans les tests** : `GameEngine` est dans `Snake.Core` (sans référence WPF), donc les tests tournent vite et sans interface.
4. **Valeurs figées pour les cas déterministes** : grille, taille, longueur du serpent sont fixes pour que les assertions soient stables (sauf `SpawnFood`, qui utilise un tirage aléatoire, mais on ne vérifie que « un fruit est placé dans la grille »).

---

## 6. Pour aller plus loin

- **Couverture** : le projet Snake.Tests inclut `coverlet.collector` ; on peut mesurer la couverture de code avec des options appropriées (`--collect:"XPlat Code Coverage"`, etc. selon l’outil).
- **Autres types de tests** : tests d’intégration (moteur + ViewModel, sans fenêtre), ou tests de composants UI, sont possibles mais plus lourds ; pour l’instant on se concentre sur la logique du `GameEngine`.

---

*Voir aussi : [README.md](README.md) pour l’architecture du projet et la structure des dossiers.*
