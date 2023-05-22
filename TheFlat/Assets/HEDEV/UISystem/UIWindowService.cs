using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.UISystem;

namespace UI
{
    public class UIControllerContainer
    {
        private readonly Dictionary<int, UIController> _controllers;
        public UIType UIType = UIType.None;

        public UIControllerContainer()
        {
            _controllers = new Dictionary<int, UIController>();
        }

        public bool IsHasBodies(int id)
        {
            return _controllers.ContainsKey(id);
        }

        public UIController GetBody(int id)
        {
            return _controllers.ContainsKey(id) ? _controllers[id] : null;
        }

        public void RegisterBody(int id, UIController body)
        {
            if (_controllers.ContainsKey(id))
                return;
            _controllers[id] = body;
            UIType = body.UIType;
            UIWindowService.CreatedControllers.Add(body);
        }

        public int RemoveBody(int id)
        {
            if (!_controllers.ContainsKey(id))
                return _controllers.Count;
            
            UIWindowService.CreatedControllers.Remove(_controllers[id]);
            _controllers.Remove(id);
            return _controllers.Count;
        }

        public List<UIController> CloseAll()
        {
            return _controllers.Select(windowBody => windowBody.Value).ToList();
        }

        public void HideAll()
        {
            foreach (var windowBody in _controllers) windowBody.Value.Hide();
        }

        public void ShowAll()
        {
            foreach (var windowBody in _controllers) windowBody.Value.Show();
        }
    }

    public static class UIWindowService
    {
        public static readonly List<UIController> CreatedControllers = new();
        public static readonly Dictionary<Type, UIControllerContainer> CreatedControllerContainer = new();

        public static void OpenWindow<T>(Action<T> receivedWindowBody = null, int id = 0,
            WindowOption option = WindowOption.None)
            where T : UIController, new()
        {
            var isAlreadyBody = false;
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
                isAlreadyBody = CreatedControllerContainer[typeof(T)].IsHasBodies(id);
            else
                CreatedControllerContainer[typeof(T)] = new UIControllerContainer();

            if (isAlreadyBody)
            {
                var body = CreatedControllerContainer[typeof(T)].GetBody(id);
                receivedWindowBody?.Invoke(body as T);
                body.OnOpen();
            }
            else
            {
                var windowController = new T();
                CreatedControllerContainer[typeof(T)].RegisterBody(id, windowController);
                windowController.SetWindowOption(id, body =>
                {
                    receivedWindowBody?.Invoke(body as T);
                    body.OnOpen();
                }, option);
            }
        }

        public static void GetWindow<T>(Action<T> windowBody = null, int id = 0,
            WindowOption option = WindowOption.None)
            where T : UIController, new()
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                if (CreatedControllerContainer[typeof(T)].IsHasBodies(id))
                {
                    var body = CreatedControllerContainer[typeof(T)].GetBody(id);
                    windowBody?.Invoke(body as T);
                }
            }
            else
            {
                OpenWindow(windowBody, id, option);
            }
        }

        public static T GetUIController<T>(int id = 0) where T : UIController
        {
            if (!CreatedControllerContainer.ContainsKey(typeof(T)))
                return null;

            var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
            return getBody as T;
        }

        public static string GetPath<T>() where T : UIWindow
        {
            return UIWindowManager.Instance ? UIWindowManager.GetUIPath<T>() : null;
        }

        public static bool IsOpenedController<T>(int id = 0) where T : UIController
        {
            if (!CreatedControllerContainer.ContainsKey(typeof(T)))
                return false;

            var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
            return getBody.IsOpenedWindow;
        }

        public static void Close<T>(int id = 0) where T : UIController
        {
            if (!CreatedControllerContainer.ContainsKey(typeof(T)))
                return;

            var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
            if (getBody == null)
                return;

            getBody.CloseAction();
            var bodyCount = CreatedControllerContainer[typeof(T)].RemoveBody(id);
            if (bodyCount <= 0)
                CreatedControllerContainer.Remove(typeof(T));
            getBody.OnDestroy();
        }

        public static void Close(UIController controller)
        {
            if (!CreatedControllerContainer.ContainsKey(controller.GetType()))
                return;

            var getBody = CreatedControllerContainer[controller.GetType()].GetBody(controller.ID);
            if (getBody == null)
                return;

            getBody.CloseAction();
            var bodyCount = CreatedControllerContainer[controller.GetType()].RemoveBody(controller.ID);
            if (bodyCount <= 0)
                CreatedControllerContainer.Remove(controller.GetType());
            getBody.OnDestroy();
        }

        public static void CloseByType(UIType uiType)
        {
            var closeTargetBodies = new List<UIController>();

            foreach (var container in CreatedControllerContainer)
            {
                if (container.Value.UIType != uiType)
                    continue;
                var bodiesList = container.Value.CloseAll();
                closeTargetBodies.AddRange(bodiesList);
            }

            foreach (var uiController in closeTargetBodies)
                Close(uiController);
        }

        public static void CloseAll()
        {
            var closeTargetBodies = new List<UIController>();

            foreach (var container in CreatedControllerContainer)
            {
                var bodiesList = container.Value.CloseAll();
                for (var i = 0; i < bodiesList.Count; i++) closeTargetBodies.Add(bodiesList[i]);
            }

            for (var i = 0; i < closeTargetBodies.Count; i++) Close(closeTargetBodies[i]);
        }

        public static void ShowWindow<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                getBody.Show();
            }
        }

        public static void HideWindow<T>(int id = 0) where T : UIController
        {
            if (CreatedControllerContainer.ContainsKey(typeof(T)))
            {
                var getBody = CreatedControllerContainer[typeof(T)].GetBody(id);
                getBody.Hide();
            }
        }

        public static UIController Find(Func<UIController, bool> func)
        {
            return CreatedControllers.FirstOrDefault(func);
        }
    }
}