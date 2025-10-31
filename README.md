# Code Challenge

## Se requiere crear una API RESTFul, con las siguientes funcionalidades:

- Permitir hacer login con nombre de usuario y clave. Debe devolver un token de expiración automática (por default de 5 minutos pero debe ser parametrizable).
- Permitir registrar/actualizar un empleado. El API debe esperar un json válido con atributos mínimos: nombre, email, supervisor_id.
- Obtener los detalles de todos los empleados y sus datetime de última actualización.
- Obtener los detalles de un empleado dado su ID. Además de los atributos indicados, el detalle por ID debe indicar: datetime de última actualización y cantidad de empleados a cargo (directos o indirectos). La obtención de detalles del empleado y su cantidad de personal a cargo debe hacerse en forma paralela con objetivo de responder más rápidamente el request.
- Todos los endpoints excepto el de login deben validar el token de sesión y devolver un error adecuado si no hay token, o el mismo es inválido/caducado.
- El código deberá estar subido a github/gitlab y se debe pasar link acceso.
- Se debe proveer forma de probar todos los endpoints.