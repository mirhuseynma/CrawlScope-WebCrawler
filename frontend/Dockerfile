FROM node:22-alpine AS build

WORKDIR /app

# Install pnpm (pin to v9 to avoid strict build script errors in CI)
RUN npm install -g pnpm@9

COPY package.json pnpm-lock.yaml* pnpm-workspace.yaml* ./
RUN pnpm install

# Pass API URL as build arg
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

COPY . .
RUN pnpm build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

# Replace default nginx config with one that supports React Router and Subdomain Routing
RUN echo 'server { \
    listen 80; \
    server_name ~^api\.; \
    location / { \
        proxy_pass http://backend:8080; \
        proxy_set_header Host $host; \
        proxy_set_header X-Real-IP $remote_addr; \
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for; \
    } \
} \
server { \
    listen 80 default_server; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
