### Blog Personal
 
Proyecto de blog personal simple desarrollado en .NET. Lo hice al mismo tiempo que aprendía sobre API's y html/css. Básicamente permite a un usuario administrador crear, editar y eliminar posts, y a los visitantes simplemente leer los posts. Para mantenerlo simple, preferí guardar todo en .json y no usar una base de datos avanzada. Tanto la parte js del frontend como algunos aspectos de los test los hice con ayuda de un LLM, el resto es fruto de la lectura, el análisis de proyectos similares y el esfuerzo por entender.

### Funcionalidades generales

*   **Gestión de Posts**: El administrador puede crear, editar y eliminar posts a través de un panel de administración. El acceso (Usuario y Contraseña) es configurable.
*   **Visualización de Posts**: Los visitantes pueden ver una lista de todos los posts y hacer clic en cada uno para leer el contenido completo.
*   **Autenticación de Administrador**: El panel de administración está protegido por un sistema de inicio de sesión basado en JWT.

### Estructura

*   **`blog-personal`**: El proyecto principal de ASP.NET Core.
    *   **`Controllers`**: Contiene los controladores de la API para los posts y la autenticación.
    *   **`Models`**: Contiene los modelos de datos para `Post` y `AdminUser`.
    *   **`Servicios`**: Contiene la lógica de negocio para la gestión de posts y la autenticación.
    *   **`wwwroot`**: Contiene los archivos estáticos del frontend (HTML, CSS, JS).
        *   **`admin`**: Contiene las páginas de administración (login, dashboard, etc.).
        *   **`data`**: Contiene los archivos de datos JSON para los posts y el usuario administrador.

### Endpoints

*   `GET /api/post`: Obtiene todos los posts.
*   `GET /api/post/{id}`: Obtiene un post por su ID.
*   `POST /api/post`: Crea un nuevo post (requiere autenticación de administrador).
*   `PUT /api/post/{id}`: Actualiza un post (requiere autenticación de administrador).
*   `DELETE /api/post/{id}`: Elimina un post (requiere autenticación de administrador).
*   `POST /api/auth/login`: Autentifica al administrador y devuelve un token JWT.

### En resumen:

*   **`backend`** → .NET, C#, ASP.NET Core.
    *   **`auth`** → JWT.
    *   **`data`** → sistema de archivos JSON.
* **`frontend`** → HTML, CSS y JS.
* **`testing`** → tests de integración.



