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
COPY --from=build /app/dist /usr/share/nginx/html
COPY OPS.Site/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
