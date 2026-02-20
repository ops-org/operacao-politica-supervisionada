# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY OPS.Site/package*.json ./
RUN npm install

# Copy the rest of the frontend code
COPY OPS.Site/ ./

# Build the app
RUN npm run build

# Final stage: Serve with Nginx
FROM nginx:alpine

# Create a non-root user for Nginx
RUN touch /var/run/nginx.pid && \
    chown -R nginx:nginx /var/run/nginx.pid /var/cache/nginx /var/log/nginx /etc/nginx/conf.d

USER nginx

COPY --from=build /app/dist /usr/share/nginx/html
COPY OPS.Site/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
