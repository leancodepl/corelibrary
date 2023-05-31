apiVersion: apps/v1
kind: Deployment
metadata:
  name: exampleapp-api
  namespace: ${NAMESPACE}
  labels:
    app: exampleapp
    component: api
    environment: ${ENVIRONMENT}
spec:
  selector:
    matchLabels:
      app: exampleapp
      component: api
      environment: ${ENVIRONMENT}
  replicas: 1
  revisionHistoryLimit: 3
  template:
    metadata:
      labels:
        app: exampleapp
        component: api
        environment: ${ENVIRONMENT}
        aadpodidbinding: exampleapp-api
    spec:
      containers:
        - name: api
          image: leancode.azurecr.io/exampleapp-api:${APP_VERSION}
          resources:
            requests:
              cpu: 200m
              memory: 200Mi
            limits:
              cpu: 500m
              memory: 300Mi
          envFrom:
            - secretRef:
                name: exampleapp-api
          env:
            - name: AGENT_HOST_IP
              valueFrom:
                fieldRef:
                  apiVersion: v1
                  fieldPath: status.hostIP
            - name: Telemetry__OtlpEndpoint
              value: http://$(AGENT_HOST_IP):55680
          ports:
            - containerPort: 80
          livenessProbe:
            httpGet:
              path: /live/health
              port: 80
            initialDelaySeconds: 15
            periodSeconds: 5
            timeoutSeconds: 5
          readinessProbe:
            httpGet:
              path: /live/ready
              port: 80
            initialDelaySeconds: 20
            periodSeconds: 10
            timeoutSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: exampleapp-api
  namespace: ${NAMESPACE}
  labels:
    app: exampleapp
    component: api
    environment: ${ENVIRONMENT}
spec:
  type: ClusterIP
  ports:
    - port: 80
  selector:
    app: exampleapp
    component: api
    environment: ${ENVIRONMENT}
