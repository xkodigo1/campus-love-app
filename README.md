# Campus Love

## 📝 Descripción

Campus Love es una aplicación de citas especialmente diseñada para la comunidad universitaria. Facilita conexiones entre estudiantes basadas en intereses comunes, carreras, y preferencias personales. La aplicación utiliza un algoritmo de emparejamiento que considera factores relevantes como área de estudio, intereses y ubicación dentro del campus.

## ✨ Características Principales

### Para Usuarios:
- **Perfiles Personalizados**: Crea tu perfil destacando tu carrera, intereses y aspiraciones.
- **Sistema de Matching**: Descubre perfiles compatibles y expresa tu interés con likes.
- **Sistema de Créditos**: Usa créditos diarios para enviar likes a perfiles que te interesen.
- **Enriquecimiento de Perfil**: Añade información detallada sobre tus gustos, libros favoritos, películas y más.
- **Chat Integrado**: Conversa con tus matches en tiempo real.
- **Estadísticas Personalizadas**: Visualiza datos sobre tu actividad, popularidad y compatibilidad.

### Para Administradores:
- **Gestión de Usuarios**: Verificación, suspensión y eliminación de cuentas.
- **Estadísticas Detalladas**: Análisis demográficos, interacciones, actividad y más.
- **Configuración del Sistema**: Ajuste de parámetros como créditos diarios, algoritmos de matching y tiempos de sesión.
- **Respaldo y Restauración**: Herramientas para proteger la integridad de los datos.

## 🔧 Requisitos Técnicos

- **.NET 7.0** o superior
- **MySQL Server** 8.0 o superior
- Mínimo 4GB de RAM
- 100MB de espacio en disco para la aplicación (+ espacio para la base de datos)
- Sistema operativo: Windows, MacOS o Linux compatible con .NET

## 🏗️ Arquitectura del Sistema

Campus Love sigue una arquitectura de capas basada en principios de Domain-Driven Design (DDD) y Clean Architecture:

### Capas de la aplicación:
1. **Capa de Dominio** (domain/): Contiene las entidades centrales del negocio y sus reglas.
2. **Capa de Aplicación** (application/): Gestiona la lógica de la aplicación y orquesta el flujo de datos.
3. **Capa de Infraestructura** (infrastructure/): Implementa las interfaces definidas en el dominio para interactuar con sistemas externos.
4. **Capa de Presentación** (application/ui/): Maneja la interacción con el usuario.

### Flujo de Datos:
```
[Usuario] <--> [UI (ConsoleUI/AdminUI)] <--> [Servicios de Aplicación] <--> [Repositorios] <--> [Base de Datos]
```

## 📊 Modelo de Datos

Campus Love se basa en las siguientes entidades principales:

### Entidades Principales:
- **User**: Información básica del usuario (ID, nombre, edad, género, etc.)
- **UserAccount**: Datos de acceso (username, email, contraseña, estado)
- **Gender**: Géneros disponibles en la plataforma
- **Career**: Carreras universitarias
- **SexualOrientation**: Orientaciones sexuales
- **City**: Ciudades para la ubicación
- **Interaction**: Interacciones entre usuarios (likes, dislikes)
- **Match**: Conexiones entre usuarios que se han dado like mutuamente
- **Conversation**: Canal de comunicación entre usuarios con match
- **Message**: Mensajes enviados en una conversación
- **Administrator**: Usuarios con privilegios de administración

### Relaciones clave:
- Un User tiene un UserAccount (1:1)
- Un User tiene múltiples Interactions (1:N)
- Un User puede estar en múltiples Matches (N:M)
- Un Match puede tener una Conversation (1:1)
- Una Conversation tiene múltiples Messages (1:N)

### Diagrama ER simplificado:
```
User (1) --- (1) UserAccount
User (1) --- (N) Interaction
User (N) --- (M) Matches --- (N) User
Match (1) --- (1) Conversation
Conversation (1) --- (N) Message
```

## 🔄 Flujos de Trabajo Principales

### 1. Flujo de Registro y Creación de Perfil

```
┌──────────────┐     ┌─────────────┐     ┌─────────────────────┐     ┌──────────────┐
│ Usuario      │     │ Validación   │     │ Creación de Cuenta  │     │ Creación     │
│ proporciona  │────>│ de datos de  │────>│ (UserAccount +     │────>│ de perfil    │
│ información  │     │ registro     │     │  hashing password)  │     │ básico       │
└──────────────┘     └─────────────┘     └─────────────────────┘     └──────────────┘
                                                                            │
                                                                            ▼
┌───────────────────┐     ┌──────────────────┐     ┌────────────────┐     ┌──────────────┐
│ Asignación        │     │ Asignación de     │     │ Verificación   │     │ Selección de │
│ inicial de        │<────│ créditos diarios  │<────│ opcional de    │<────│ preferencias │
│ matches potenciales│     │ (user_credits)   │     │ cuenta         │     │ de matching  │
└───────────────────┘     └──────────────────┘     └────────────────┘     └──────────────┘
```

### 2. Flujo de Matching e Interacción

```
┌──────────────┐     ┌─────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│ Visualización│     │ Usuario da  │     │ Verificación y      │     │ Registro de      │
│ de perfiles  │────>│ like/dislike│────>│ reducción de        │────>│ interacción en   │
│ compatibles  │     │             │     │ créditos            │     │ base de datos    │
└──────────────┘     └─────────────┘     └─────────────────────┘     └──────────────────┘
                                                                              │
                                                                              ▼
┌───────────────────┐     ┌──────────────────┐     ┌────────────────┐     ┌──────────────┐
│ Creación          │     │ Creación de       │     │ Notificación   │     │ ¿Match?      │
│ automática de     │<────│ conversación      │<────│ a ambos        │<────│ (like mutuo) │
│ estadísticas      │     │                   │     │ usuarios       │     │              │
└───────────────────┘     └──────────────────┘     └────────────────┘     └──────────────┘
```

### 3. Flujo del Sistema de Chat

```
┌──────────────┐     ┌─────────────────┐     ┌─────────────────────┐     ┌──────────────┐
│ Usuario      │     │ Verificación    │     │ Almacenamiento      │     │ Entrega      │
│ envía        │────>│ de match        │────>│ en base de datos    │────>│ al           │
│ mensaje      │     │ existente       │     │ (tabla Messages)    │     │ destinatario │
└──────────────┘     └─────────────────┘     └─────────────────────┘     └──────────────┘
                                                                                │
                                                                                ▼
┌───────────────────┐     ┌──────────────────────────┐
│ Actualización     │     │ Marcado como leído       │
│ de estadísticas   │<────│ cuando el destinatario   │
│ de comunicación   │     │ abre la conversación     │
└───────────────────┘     └──────────────────────────┘
```

### 4. Flujo Administrativo

```
┌──────────────┐     ┌─────────────────────────┐     ┌─────────────────────┐
│ Admin        │     │ Validación con          │     │ Acceso al panel     │
│ ingresa      │────>│ tabla Administrators    │────>│ administrativo      │
│ credenciales │     │                         │     │                     │
└──────────────┘     └─────────────────────────┘     └─────────────────────┘
                                                               │
                                 ┌─────────────────────────────┼─────────────────────────────┐
                                 ▼                             ▼                             ▼
                       ┌─────────────────┐           ┌─────────────────┐           ┌─────────────────┐
                       │ Gestión de      │           │ Consulta de     │           │ Configuración   │
                       │ usuarios        │           │ estadísticas    │           │ del sistema     │
                       └─────────────────┘           └─────────────────┘           └─────────────────┘
```

### 5. Flujo del Sistema de Créditos

```
┌──────────────────┐     ┌───────────────────┐     ┌─────────────────────┐
│ Tarea programada │     │ Recarga diaria    │     │ Actualización de    │
│ (00:00 UTC)      │────>│ de créditos       │────>│ user_credits        │
│                  │     │ (10 créditos)     │     │                     │
└──────────────────┘     └───────────────────┘     └─────────────────────┘

┌──────────────────┐     ┌───────────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│ Usuario da like  │     │ Verificación de   │     │ Reducción de        │     │ Registro en      │
│ a un perfil      │────>│ créditos          │────>│ crédito (1 unidad)  │────>│ tabla DailyCredits│
│                  │     │ disponibles       │     │                     │     │                  │
└──────────────────┘     └───────────────────┘     └─────────────────────┘     └──────────────────┘
```

## 🚀 Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/xkodigo1/campus-love-app.git
   cd campus-love-app
   ```

2. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

3. **Configurar la base de datos**:
   - Crea una base de datos MySQL
   - Actualiza la cadena de conexión en `appsettings.json`
   - Ejecuta el script de inicialización:
   ```bash
   mysql -u username -p database_name < infrastructure/mysql/init.sql
   ```

4. **Compilar y ejecutar la aplicación**:
   ```bash
   dotnet build
   dotnet run
   ```

## 📖 Uso de la Aplicación

### Acceso
Al iniciar la aplicación, se presenta una pantalla de selección entre acceso de Usuario o Administrador.

### Usuarios
1. **Registro/Login**: Crea una cuenta con email y contraseña o inicia sesión si ya tienes cuenta.
2. **Crear Perfil**: Completa tu información personal, preferencias y añade una foto.
3. **Explorar Perfiles**: Navega entre los perfiles disponibles y da like a los que te interesen.
4. **Gestiona tus Matches**: Revisa tus matches y comienza conversaciones.
5. **Consulta Estadísticas**: Visualiza datos sobre tu actividad y popularidad.

### Administradores
1. **Login**: Accede con credenciales administrativas (predeterminado: admin/admin123).
2. **Gestión de Usuarios**: Verifica, suspende o elimina cuentas.
3. **Estadísticas**: Consulta datos demográficos, de interacción y uso.
4. **Configuraciones**: Ajusta parámetros del sistema según las necesidades.

## 📊 Sistema de Créditos

- Cada usuario recibe 10 créditos diarios a las 00:00 UTC
- Cada like consume 1 crédito
- Se otorgan 5 créditos adicionales por completar el perfil enriquecido
- Los créditos no utilizados se acumulan hasta un máximo de 30

### Tablas relacionadas con créditos:
- **user_credits**: Almacena el balance actual de créditos de cada usuario
- **DailyCredits**: Registra el historial de uso diario de créditos

## 🤝 Algoritmo de Matching

El algoritmo de matching en Campus Love se basa en un sistema ponderado que considera:

- **Compatibilidad de edad**: 25% (basado en el rango de edad preferido)
- **Compatibilidad de carrera**: 20% (carreras afines tienen mayor puntuación)
- **Proximidad geográfica**: 20% (usuarios en la misma ciudad o campus)
- **Intereses comunes**: 15% (hobbies, gustos musicales, películas, etc.)
- **Preferencias de género**: 20% (orientación sexual y género)

La puntuación final determina el orden en que se muestran los perfiles. Los perfiles con mayor puntuación aparecen primero en las recomendaciones.

## 💾 Estructura de la Base de Datos

### Tablas Principales:
- **Users**: Almacena información personal y preferencias
- **UserAccounts**: Gestiona credenciales y estado de la cuenta
- **Genders**: Catálogo de géneros disponibles
- **Careers**: Catálogo de carreras universitarias
- **SexualOrientations**: Catálogo de orientaciones sexuales
- **Cities**: Ubicaciones disponibles
- **Interactions**: Registro de likes/dislikes entre usuarios
- **Matches**: Conexiones exitosas (likes mutuos)
- **Conversations**: Canales de comunicación entre usuarios con match
- **Messages**: Mensajes individuales en conversaciones
- **Administrators**: Cuentas con privilegios administrativos
- **user_credits**: Balance de créditos de usuarios
- **DailyCredits**: Historial de uso de créditos

### Índices clave:
- Primary keys en todas las tablas
- Foreign keys para relaciones
- Índices en campos de búsqueda frecuente (UserID, Username, Email)
- Índices compuestos en Interactions (FromUserID, ToUserID)
- Índices en fechas para consultas temporales

## 👨‍💻 Estructura del Proyecto

```
campus-love-app/
├── application/              # Lógica de aplicación
│   ├── services/             # Servicios de negocio
│   │   ├── LoginService.cs   # Autenticación y registro
│   │   └── ...
│   └── ui/                   # Interfaces de usuario
│       ├── ConsoleUI.cs      # UI para usuarios regulares
│       ├── AdminUI.cs        # UI para administradores
│       └── ...
├── domain/                   # Entidades y reglas de negocio
│   ├── entities/             # Modelos de dominio
│   │   ├── User.cs           # Entidad de usuario
│   │   ├── Match.cs          # Entidad de match
│   │   ├── Message.cs        # Entidad de mensaje
│   │   └── ...
│   └── ports/                # Interfaces de repositorios
│       ├── IUserRepository.cs
│       ├── IChatRepository.cs
│       └── ...
├── infrastructure/           # Implementaciones técnicas
│   ├── mysql/                # Scripts de base de datos
│   │   ├── init.sql          # Script de inicialización
│   │   └── ...
│   ├── repositories/         # Implementación de repositorios
│   │   ├── UserRepository.cs
│   │   ├── ChatRepository.cs
│   │   └── ...
│   └── mysql/                # Conexión a base de datos
│       └── SingletonConnection.cs
└── Program.cs                # Punto de entrada de la aplicación
```

## 🔄 Procesos Internos

### Gestión de Sesiones
La aplicación maneja las sesiones de usuario a través de objetos almacenados en memoria durante la ejecución. Cuando un usuario inicia sesión, sus datos se mantienen en los objetos `_currentUser` y `_currentAccount` en la clase `ConsoleUI`.

### Verificación de Usuarios
Los administradores pueden marcar cuentas como "verificadas" para indicar que han confirmado la autenticidad del usuario. Esto se refleja en el campo `IsVerified` de la tabla Users y afecta cómo se muestran estos perfiles (con un indicador visual).

### Sistemas de Notificación
Cuando ocurre un match o se recibe un mensaje, la aplicación muestra notificaciones en la interfaz de consola. No se implementan notificaciones persistentes fuera de la sesión activa.

### Proceso de Matching
1. El usuario ve un perfil y decide dar like/dislike
2. Se consume un crédito por cada like (los dislikes son gratuitos)
3. La interacción se registra en la tabla Interactions
4. Si ambos usuarios se han dado like mutuamente:
   - Se crea un registro en la tabla Matches
   - Se crea una conversación asociada en tabla Conversations
   - Se notifica a ambos usuarios

## 🛠️ Tecnologías Utilizadas

- **Lenguaje**: C# (.NET 9)
- **Base de Datos**: MySQL
- **UI Framework**: Console (Spectre.Console)
- **ORM**: ADO.NET nativo
- **Autenticación**: Hash de contraseñas (SHA256)
- **Patrones de Diseño**:
  - Singleton (conexión a BD)
  - Repository (acceso a datos)
  - Dependency Injection (manual)
  - Service Layer (separación de lógica)

## 🔒 Seguridad

- **Almacenamiento de contraseñas**: Las contraseñas se guardan con hash SHA256
- **Validación de datos**: Todos los inputs son validados antes de procesarse
- **Control de acceso**: Separación estricta entre roles de usuario y administrador
- **Sesiones**: Timeout configurable para sesiones inactivas
- **Bloqueo de cuentas**: Después de múltiples intentos fallidos de login
- **Sanitización de datos**: Prevención de SQL injection en todas las consultas

## 🔧 Mantenimiento

### Respaldo de Base de Datos
El sistema incluye funcionalidad de respaldo automatizado diario. Los administradores pueden:
- Ver el historial de respaldos
- Crear respaldos manuales
- Restaurar desde respaldos previos

### Monitoreo
El panel administrativo incluye estadísticas en tiempo real sobre:
- Usuarios activos
- Tasas de matching
- Utilización de créditos
- Actividad de chat

### Depuración
Para depuración, se pueden habilitar logs detallados modificando el valor `EnableDebugMode` en el archivo `appsettings.json`.

## 📜 Licencia

© 2025 Campus Love. Todos los derechos reservados.

---

Desarrollado con ❤️ para la comunidad.