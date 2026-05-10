# Build stage
FROM node:20-alpine AS build
WORKDIR /app

ARG BUILD_CONFIG=production

# Instalar dependencias
COPY package*.json ./
RUN npm ci

# Copiar código y compilar
COPY . .
RUN npm run build -- --configuration ${BUILD_CONFIG}

# Serve stage con Nginx
FROM nginx:alpine AS runtime
COPY --from=build /app/dist/*/browser /usr/share/nginx/html

# Configuración de Nginx para Angular (SPA routing)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
