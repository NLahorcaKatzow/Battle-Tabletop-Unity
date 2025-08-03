using UnityEngine;

/// <summary>
/// Utilidades de álgebra vectorial para cálculos de orientación y movimiento en superficies
/// </summary>
public static class VectorialUtil
{
    #region Surface Orientation Calculations
    
    /// <summary>
    /// Calcula un vector "forward" ortogonal al vector "up" dado
    /// </summary>
    /// <param name="upVector">Vector que apunta hacia arriba (perpendicular a la superficie)</param>
    /// <returns>Vector forward calculado usando producto cruzado</returns>
    public static Vector3 CalculateForwardVector(Vector3 upVector)
    {
        // Usar el vector forward mundial como referencia inicial
        Vector3 worldForward = Vector3.forward;
        
        // Si el vector up es paralelo al forward mundial, usar right como referencia
        if (Vector3.Dot(upVector, worldForward) > 0.99f)
        {
            worldForward = Vector3.right;
        }
        
        // Calcular forward usando producto cruzado para asegurar ortogonalidad
        Vector3 tempRight = Vector3.Cross(upVector, worldForward).normalized;
        Vector3 forwardVector = Vector3.Cross(tempRight, upVector).normalized;
        
        return forwardVector;
    }
    
    /// <summary>
    /// Calcula una base ortonormal completa a partir de una normal de superficie
    /// Z apunta perpendicular a la superficie, XY es el plano de movimiento
    /// </summary>
    /// <param name="surfaceNormal">Normal de la superficie</param>
    /// <returns>Tupla con (right, forward, up) vectores ortonormales</returns>
    public static (Vector3 right, Vector3 forward, Vector3 up) CalculateOrthonormalBasis(Vector3 surfaceNormal)
    {
        Vector3 up = surfaceNormal; // Z apunta perpendicular a la superficie (normal)
        
        // Crear un sistema de coordenadas consistente basado en la normal
        Vector3 forward, right;
        
        // Determinar qué eje mundial usar como referencia basado en la normal
        Vector3 worldReference;
        
        // Si la normal es principalmente vertical (Y), usar X como referencia
        if (Mathf.Abs(Vector3.Dot(up, Vector3.up)) > 0.9f)
        {
            worldReference = Vector3.right;
        }
        // Si la normal es principalmente horizontal en X, usar Y como referencia
        else if (Mathf.Abs(Vector3.Dot(up, Vector3.right)) > 0.9f)
        {
            worldReference = Vector3.up;
        }
        // Si la normal es principalmente horizontal en Z, usar Y como referencia
        else if (Mathf.Abs(Vector3.Dot(up, Vector3.forward)) > 0.9f)
        {
            worldReference = Vector3.up;
        }
        // Para casos intermedios, usar Y como referencia por defecto
        else
        {
            worldReference = Vector3.up;
        }
        
        // Calcular right usando producto cruzado
        right = Vector3.Cross(up, worldReference).normalized;
        
        // Si right es cero (vectores paralelos), usar una referencia alternativa
        if (right.magnitude < 0.1f)
        {
            worldReference = Vector3.forward;
            right = Vector3.Cross(up, worldReference).normalized;
        }
        
        // Calcular forward usando producto cruzado para completar la base ortonormal
        forward = Vector3.Cross(right, up).normalized;
        
        return (right, forward, up);
    }
    
    /// <summary>
    /// Crea una rotación que alinea un objeto con una superficie
    /// Z será perpendicular a la superficie, XY será el plano de movimiento
    /// </summary>
    /// <param name="surfaceNormal">Normal de la superficie</param>
    /// <returns>Quaternion de rotación para alinearse con la superficie</returns>
    public static Quaternion CalculateSurfaceAlignment(Vector3 surfaceNormal)
    {
        var (right, forward, up) = CalculateOrthonormalBasis(surfaceNormal);
        return Quaternion.LookRotation(forward, up);
    }
    
    #endregion
    
    #region Movement Calculations
    
    /// <summary>
    /// Convierte input 2D a movimiento 3D en el plano XY local
    /// </summary>
    /// <param name="input">Input del jugador (x = horizontal A/D, y = vertical W/S)</param>
    /// <param name="localTransform">Transform del objeto para convertir a coordenadas mundiales</param>
    /// <returns>Vector de movimiento en coordenadas mundiales</returns>
    public static Vector3 ConvertInputToXYPlaneMovement(Vector2 input, Transform localTransform)
    {
        // CORREGIDO: Mapeo correcto del input
        // input.x (A/D) -> transform.right (eje X local)
        // input.y (W/S) -> transform.up (eje Y local)
        // Z local (transform.forward) es perpendicular al plano de movimiento
        
        Vector3 rightMovement = localTransform.right * input.x;    // A/D -> derecha/izquierda
        Vector3 upMovement = localTransform.up * input.y;          // W/S -> arriba/abajo
        
        Vector3 totalMovement = rightMovement + upMovement;
        return totalMovement.normalized;
    }
    
    /// <summary>
    /// Separa una velocidad en componentes paralelas y perpendiculares a una superficie
    /// </summary>
    /// <param name="velocity">Velocidad a separar</param>
    /// <param name="surfaceNormal">Normal de la superficie</param>
    /// <returns>Tupla con (parallel, perpendicular) componentes de velocidad</returns>
    public static (Vector3 parallel, Vector3 perpendicular) SeparateVelocityComponents(Vector3 velocity, Vector3 surfaceNormal)
    {
        Vector3 parallel = Vector3.ProjectOnPlane(velocity, surfaceNormal);
        Vector3 perpendicular = velocity - parallel;
        
        return (parallel, perpendicular);
    }
    
    /// <summary>
    /// Calcula la fuerza necesaria para alcanzar una velocidad objetivo
    /// </summary>
    /// <param name="currentVelocity">Velocidad actual</param>
    /// <param name="targetVelocity">Velocidad objetivo</param>
    /// <param name="forceMultiplier">Multiplicador de fuerza</param>
    /// <returns>Vector de fuerza a aplicar</returns>
    public static Vector3 CalculateForceToReachVelocity(Vector3 currentVelocity, Vector3 targetVelocity, float forceMultiplier)
    {
        Vector3 velocityChange = targetVelocity - currentVelocity;
        return velocityChange * forceMultiplier;
    }
    
    #endregion
    
    #region Gravity Calculations
    
    /// <summary>
    /// Calcula la dirección de gravedad artificial basada en la superficie
    /// La gravedad apunta hacia la superficie (opuesto a la normal)
    /// </summary>
    /// <param name="surfaceNormal">Normal de la superficie</param>
    /// <returns>Dirección hacia donde debe apuntar la gravedad</returns>
    public static Vector3 CalculateGravityDirection(Vector3 surfaceNormal)
    {
        // La gravedad apunta hacia la superficie (opuesto a la normal)
        return -surfaceNormal;
    }
    
    /// <summary>
    /// Calcula la fuerza de gravedad artificial
    /// </summary>
    /// <param name="gravityDirection">Dirección de la gravedad</param>
    /// <param name="gravityStrength">Intensidad de la gravedad</param>
    /// <returns>Vector de fuerza gravitacional</returns>
    public static Vector3 CalculateGravityForce(Vector3 gravityDirection, float gravityStrength)
    {
        return gravityDirection.normalized * gravityStrength;
    }
    
    #endregion
    
    #region Surface Detection Utilities
    
    /// <summary>
    /// Calcula la posición corregida para mantener distancia mínima de una superficie
    /// </summary>
    /// <param name="currentPosition">Posición actual</param>
    /// <param name="contactPoint">Punto de contacto con la superficie</param>
    /// <param name="surfaceNormal">Normal de la superficie</param>
    /// <param name="minDistance">Distancia mínima a mantener</param>
    /// <returns>Posición corregida</returns>
    public static Vector3 CalculateCorrectedPosition(Vector3 currentPosition, Vector3 contactPoint, Vector3 surfaceNormal, float minDistance)
    {
        float distanceToSurface = Vector3.Distance(currentPosition, contactPoint);
        
        if (distanceToSurface < minDistance)
        {
            return contactPoint + surfaceNormal * minDistance;
        }
        
        return currentPosition;
    }
    
    /// <summary>
    /// Verifica si una posición necesita corrección de distancia
    /// </summary>
    /// <param name="currentPosition">Posición actual</param>
    /// <param name="contactPoint">Punto de contacto</param>
    /// <param name="minDistance">Distancia mínima</param>
    /// <returns>True si necesita corrección</returns>
    public static bool NeedsPositionCorrection(Vector3 currentPosition, Vector3 contactPoint, float minDistance)
    {
        return Vector3.Distance(currentPosition, contactPoint) < minDistance;
    }
    
    #endregion
    
    #region Debug Utilities
    
    /// <summary>
    /// Calcula los puntos de las esquinas de un plano XY para visualización
    /// </summary>
    /// <param name="center">Centro del plano</param>
    /// <param name="right">Vector right del plano</param>
    /// <param name="forward">Vector forward del plano</param>
    /// <param name="size">Tamaño del plano</param>
    /// <returns>Array con las 4 esquinas del plano</returns>
    public static Vector3[] CalculatePlaneCorners(Vector3 center, Vector3 right, Vector3 forward, float size)
    {
        return new Vector3[]
        {
            center + right * size + forward * size,
            center + right * size - forward * size,
            center - right * size - forward * size,
            center - right * size + forward * size
        };
    }
    
    #endregion
} 