# Analyse de cohérence des modèles — AssuranceService

## Vue d'ensemble

Le service Assurance gère des polices d'assurance pour des marchandises importées, avec des modèles EF Core (Models), des entités métier (Entities) et un référentiel partagé. Cette analyse identifie les incohérences et propose des corrections.

---

## 1. Problèmes critiques

### 1.1 Cotation — modèle et migration désalignés

**Problème** : Le modèle `Cotation` déclare des FK (`ContratId`, `PartenaireId`, `UserId`, `StatutId`) de type `ulong` avec `[ForeignKey]`, alors que la migration `InitialCreate` crée une table Cotations **sans ces colonnes** :
- Montant, Taux, PrimeNette, Accessoires, Taxe, PrimeTotale
- BaseModel (CreerPar, ModifierPar, CreerLe, ModifierLe, IsDeleted)

Les tables Contrat, Partenaire, User, Statut ne sont pas créées dans la migration.

**Impact** : Insertions/lectures de Cotation échoueront ou ignoreront ces FK. Le modèle est incohérent avec le schéma réel.

**Recommandation** :
- Soit ajouter une migration pour créer les tables Contrat, Partenaire, User, Statut et les colonnes FK dans Cotations
- Soit retirer ces propriétés du modèle Cotation si ces concepts ne sont pas encore en usage

---

### 1.2 VisaAssurance — colonnes et type manquants

**Problème** :
1. Le modèle a `Assurance` (Guid) et `Partenaire` (Guid) mais la migration ne crée **aucune colonne** AssuranceId/Partenaire dans VisaAssurances
2. Le modèle a `VisaOK` en `byte` ; la migration crée `VisaOK` en `bool`
3. `Assurance` devrait s'appeler `AssuranceId` et avoir une navigation `Assurance`
4. Pas de configuration de relation `VisaAssurance` ↔ `Assurance` dans `ConfigureVisaAssurance`

**Impact** : Relation Assurance ↔ VisaAssurance non fonctionnelle. Impossible de lier un visa à une assurance.

**Recommandation** :
- Renommer `Assurance` → `AssuranceId`, `Partenaire` → `PartenaireId`
- Ajouter navigation `Assurance` et configurer la FK dans `ConfigureVisaAssurance`
- Aligner `VisaOK` sur `bool` (cohérent avec la migration) ou migrer la colonne vers `tinyint`
- Créer une migration pour ajouter AssuranceId et PartenaireId à VisaAssurances

---

### 1.3 Transport : 1-1 vs 1-n mal configuré

**Problème** : Assurance déclare `Maritime?`, `Aerien?`, `Routier?`, `Fluvial?` (un seul par assurance selon le module), mais les configurations EF (ConfigureAerien, etc.) utilisent `.WithMany()` :

```csharp
entity.HasOne(a => a.Assurance)
      .WithMany()  // ← Devrait être WithOne pour 1-1
```

Sans `WithOne`, EF suppose qu’une Assurance peut avoir plusieurs Aeriens, ce qui ne correspond pas au modèle métier.

**Recommandation** :
- Remplacer par `WithOne(a => a.Aerien)` (et idem pour Maritime, Routier, Fluvial)
- Ou passer à `ICollection<Aerien> Aeriens` si le métier impose plusieurs transports par assurance

---

## 2. Problèmes modérés

### 2.1 Doublon Policy/Customer vs Assurance

**Constat** : Deux ensembles de modèles coexistent :
- **Entities** : Policy, Customer (tables Policies, Customers)
- **Models** : Assurance, Marchandise, Prime, etc. (tables Assurances, Marchandises, etc.)

Aucun lien entre Policy et Assurance. Policy référence un CustomerId, Assurance stocke ImportateurNom, PartenaireNom en texte.

**Impact** : Risque de confusion, duplication fonctionnelle (deux façons de représenter une police).

**Recommandation** :
- Clarifier le périmètre : Policy/Customer pour quel cas d’usage ?
- Si Policy est legacy ou dédié à un autre flux, documenter clairement
- Sinon, envisager une unification (p.ex. Assurance comme implémentation de Policy)

---

### 2.2 Garantie — chaîne vs entité

**Constat** :
- `Assurance.Garantie` (string?) : texte libre
- Entité `Garantie` (ID, NomGarantie, Taux, Accessoires) : référentiel structuré

Il n’y a pas de FK entre Assurance et Garantie. La table Garanties n’est pas reliée aux Assurances.

**Impact** : Pas d’incohérence technique, mais ambiguïté métier (code, libellé ou ID ?).

**Recommandation** :
- Documenter si `Assurance.Garantie` est un code, un libellé ou un ID
- Si le référentiel Garantie est utilisé, envisager une FK `GarantieId` sur Assurance

---

### 2.3 Redondance Voyage vs Aérien/Maritime/Routier/Fluvial

**Constat** : Assurance possède à la fois :
- `Voyages` (ICollection) : modèle unifié avec TypeTransport, NomNavire, etc.
- `Maritime`, `Aerien`, `Routier`, `Fluvial` : modèles spécifiques par module

Le DbContext indique que Maritime/Aerien/Routier/Fluvial sont des "anciens modèles - à migrer vers Voyage".

**Impact** : Données potentiellement dupliquées ou contradictoires si les deux sont renseignés.

**Recommandation** :
- Planifier une migration fonctionnelle vers Voyage
- Une fois migré, supprimer les anciens modèles ou les garder en lecture seule pour l’historique

---

### 2.4 Prime — [ForeignKey] mal ciblé

**Problème** : `[ForeignKey("Assurances")]` sur AssuranceId. Le nom correct pour la navigation est généralement `Assurance` (entité), pas `Assurances` (table).

**Recommandation** : Remplacer par `[ForeignKey(nameof(Assurance))]` ou supprimer l’attribut si la convention suffit.

---

### 2.5 Marchandise — using superflu

**Problème** : `using static AssuranceService.Domain.Models.Assurance` — inutile pour les contraintes `MaxLength` (elles viennent de `System.ComponentModel.DataAnnotations`).

**Recommandation** : Supprimer ce `using`.

---

## 3. Problèmes mineurs

### 3.1 BaseModel — convention CreerLe / ModifierLe

`CreerLe` est `DateTime` (requis), `ModifierLe` est `DateTime?`. C’est logique (création obligatoire, modification optionnelle), mais à documenter pour homogénéité.

### 3.2 Cotation — types numériques

`Montant` en `double` — pour des montants financiers, `decimal` est préférable (comme dans Prime, Marchandise). La migration utilise déjà `decimal(18,2)` pour Montant.

---

## 4. Synthèse des actions

| Priorité | Élément                         | Action proposée                                 |
|----------|---------------------------------|-------------------------------------------------|
| Critique | Cotation                        | Aligner modèle et migration (FK ou suppression) |
| Critique | VisaAssurance                   | Ajouter AssuranceId/PartenaireId + config FK   |
| Critique | Transport (Aerien, etc.)        | Configurer 1-1 avec `WithOne`                  |
| Modéré   | Policy vs Assurance             | Clarifier périmètre et documenter               |
| Modéré   | Garantie vs Assurance.Garantie  | Documenter ou ajouter FK GarantieId             |
| Modéré   | Voyage vs transports anciens    | Planifier migration vers Voyage                 |
| Modéré   | Prime [ForeignKey]              | Corriger en `nameof(Assurance)`                 |
| Mineur   | Marchandise `using`             | Supprimer le `using static`                     |
| Mineur   | Cotation.Montant                | Passer à `decimal`                              |

---

## 5. Points positifs

- Relations Assurance ↔ Marchandise, Prime, Voyage correctement configurées
- Référentiel (Pays, Devises, etc.) cohérent avec DeclarationImportationService
- BaseModel (CreerPar, ModifierPar, CreerLe, ModifierLe, IsDeleted) homogène
- Transports (Aerien, Maritime, Routier, Fluvial) bien reliés à Assurance via AssuranceId
- Architecture (Api / Application / Domain / Infrastructure) bien séparée
