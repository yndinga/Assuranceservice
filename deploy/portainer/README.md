# Déploiement Portainer (Stack)

## 1) Image Docker

Le dépôt contient déjà un `Dockerfile` à la racine (recommandé pour builder l’API).

Build local :

```bash
docker build -t assurance-service:local -f Dockerfile .
```

Build + tag registre :

```bash
docker build -t REGISTRY/assurance-service:1.1.0 -f Dockerfile .
docker push REGISTRY/assurance-service:1.1.0
```

Pour une **version figée** `v1.1.0` / `1.1.0` sur GHCR, pousse le tag Git **`v1.1.0`**. Ce tag **ne se met pas à jour** quand tu pousses seulement `master` : l’image `:v1.1.0` reste celle du dernier build de ce tag. Pour suivre chaque build CI, utilise **`latest`** ou **`master`** (voir `pull_policy: always` dans le stack).

## 2) Stack Portainer

Dans Portainer → **Stacks** → **Add stack** :

- **Web editor** : coller `deploy/portainer/stack.yml`
- **Environment variables** : définir au minimum :
  - `ASSURANCE_IMAGE` (défaut dans le YAML : `ghcr.io/yndinga/assuranceservice:latest`)
  - `ASSURANCE_HTTP_PORT` (ex: `8087`)
  - `ASSURANCE_CONNECTION_STRING` (SQL Server)
  - `MINIO_ENDPOINT`, `MINIO_ACCESS_KEY`, `MINIO_SECRET_KEY`

Optionnel :
- `RABBITMQ_ENABLED=true` + `RABBITMQ_CONNECTION_STRING`
- `CONSUL_HOST`, `CONSUL_SERVICE_ADDRESS` si service discovery Consul actif

## 3) Vérification

- **API** : `GET /` et `GET /health`
- **Swagger** : `/swagger`

## 4) « Pas toujours à jour » sur Portainer — causes fréquentes

1. **La CI GitHub n’a pas encore fini** : attends la fin du workflow *Build and Push to GHCR* (plusieurs minutes).
2. **Tag d’image fixe** (`v1.1.0`, etc.) : il ne pointe que vers le build du tag Git correspondant. Pour suivre `master`, garde **`latest`** ou **`master`** dans `ASSURANCE_IMAGE` / `IMAGE_TAG`.
3. **Portainer ne recrée pas le conteneur** : même avec `pull_policy: always`, selon le mode (Docker seul vs Swarm) le comportement varie.
   - Vérifie l’option type **toujours tirer l’image** / **recreate** lors du déploiement si ton Portainer l’affiche.
   - **Force la mise à jour** : dans les variables du stack, incrémente **`DEPLOY_TRIGGER`** (0 → 1 → 2…), puis **Update the stack** : ça change la config du service et oblige généralement à recréer le conteneur.
4. **Mode Swarm** : `pull_image` / politique de pull peut être différente ; en dernier recours sur le nœud :  
   `docker service update --force --image ghcr.io/yndinga/assuranceservice:latest NOM_DU_SERVICE`
5. **En SSH sur l’hôte Docker** (diagnostic) :

```bash
docker pull ghcr.io/yndinga/assuranceservice:latest
docker compose -f /path/to/stack.yml up -d --force-recreate
```

