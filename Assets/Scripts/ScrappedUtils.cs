// private Transform DetectTradeship()
// {
//     GameObject[] ships = tradeShips;
//     if (ships == null || ships.Length == 0)
//     {
//         return null;
//     }
//     
//     Transform bestTarget = null;
//     float bestDistance = float.MaxValue;
//
//     foreach (GameObject ship in ships)
//     {
//         if (ship == null)
//         {
//             continue;
//         }
//         
//         Transform shipTransform = ship.transform;
//         
//         Vector3 directionToShip = shipTransform.position - transform.position;
//         directionToShip.y = 0;
//         
//         float shipDistance = directionToShip.magnitude;
//         Debug.Log(shipDistance);
//         if (shipDistance > viewDistance)
//         {
//             continue;
//         }
//         
//         float angleToShip = Vector3.Angle(transform.forward, directionToShip);
//         if (angleToShip > fovAngle / 2f)
//         {
//             continue;
//         }
//
//         if (Physics.Raycast(transform.position, directionToShip.normalized, out RaycastHit hit, viewDistance,
//                 obstacleMask))
//         {
//             if (hit.transform != shipTransform)
//             {
//                 continue;
//             }
//         }
//
//         if (shipDistance < bestDistance)
//         {
//             bestDistance = shipDistance;
//             bestTarget = shipTransform;
//         }
//     }
//
//     return bestTarget;
// }