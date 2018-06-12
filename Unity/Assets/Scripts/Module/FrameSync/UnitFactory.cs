using UnityEngine;

namespace ETModel
{
    public static class UnitFactory
    {
        public static Unit Create(long id)
        {
	        ResourcesComponent resourcesComponent = Game.ResourcesComponent;
	        GameObject bundleGameObject = (GameObject)resourcesComponent.GetAsset("Unit.unity3d", "Unit");
	        GameObject prefab = bundleGameObject.Get<GameObject>("Skeleton");
	        
            UnitComponent unitComponent = Game.UnitComponent;
            
	        Unit unit = ObjectFactory.CreateWithId<Unit>(id);
	        unit.GameObject = UnityEngine.Object.Instantiate(prefab);
	        GameObject parent = GameObject.Find($"/Global/Unit");
	        unit.GameObject.transform.SetParent(parent.transform, false);
			unit.AddComponent<AnimatorComponent>();
	        unit.AddComponent<MoveComponent>();

            unitComponent.Add(unit);
            return unit;
        }
    }
}