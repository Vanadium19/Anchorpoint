using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CommandsModule
{
    public abstract class ExecuteCommandButton<TAsyncCommand> : MonoBehaviour where TAsyncCommand : IAsyncCommand
    {
        [SerializeField] private Button button;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute();
    }


    public abstract class ExecuteCommandButton<TAsyncCommand, TArgument> : MonoBehaviour
        where TAsyncCommand : IAsyncCommand<TArgument>
    {
        [SerializeField] private Button button;
        [SerializeField] private TArgument argument;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;
        protected TArgument Argument => argument;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute(argument);
    }

    public abstract class ExecuteCommandButton<TAsyncCommand, TArgument1, TArgument2> : MonoBehaviour
        where TAsyncCommand : IAsyncCommand<TArgument1, TArgument2>
    {
        [SerializeField] private Button button;
        [SerializeField] private TArgument1 argument1;
        [SerializeField] private TArgument2 argument2;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;
        protected TArgument1 Argument1 => argument1;
        protected TArgument2 Argument2 => argument2;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute(argument1, argument2);
    }

    public abstract class ExecuteCommandButton<TAsyncCommand, TArgument1, TArgument2, TArgument3> : MonoBehaviour
        where TAsyncCommand : IAsyncCommand<TArgument1, TArgument2, TArgument3>
    {
        [SerializeField] private Button button;
        [SerializeField] private TArgument1 argument1;
        [SerializeField] private TArgument2 argument2;
        [SerializeField] private TArgument3 argument3;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;
        protected TArgument1 Argument1 => argument1;
        protected TArgument2 Argument2 => argument2;
        protected TArgument3 Argument3 => argument3;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute(argument1, argument2, argument3);
    }

    public abstract class ExecuteCommandButton<TAsyncCommand, TArgument1, TArgument2, TArgument3, TArgument4> : MonoBehaviour
        where TAsyncCommand : IAsyncCommand<TArgument1, TArgument2, TArgument3, TArgument4>
    {
        [SerializeField] private Button button;
        [SerializeField] private TArgument1 argument1;
        [SerializeField] private TArgument2 argument2;
        [SerializeField] private TArgument3 argument3;
        [SerializeField] private TArgument4 argument4;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;
        protected TArgument1 Argument1 => argument1;
        protected TArgument2 Argument2 => argument2;
        protected TArgument3 Argument3 => argument3;
        protected TArgument4 Argument4 => argument4;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute(argument1, argument2, argument3, argument4);
    }

    public abstract class ExecuteCommandButton<TAsyncCommand, TArgument1, TArgument2, TArgument3, TArgument4, TArgument5> : MonoBehaviour
        where TAsyncCommand : IAsyncCommand<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5>
    {
        [SerializeField] private Button button;
        [SerializeField] private TArgument1 argument1;
        [SerializeField] private TArgument2 argument2;
        [SerializeField] private TArgument3 argument3;
        [SerializeField] private TArgument4 argument4;
        [SerializeField] private TArgument5 argument5;

        [Inject] private TAsyncCommand _command;

        protected Button Button => button;
        protected TArgument1 Argument1 => argument1;
        protected TArgument2 Argument2 => argument2;
        protected TArgument3 Argument3 => argument3;
        protected TArgument4 Argument4 => argument4;
        protected TArgument5 Argument5 => argument5;

        private void OnEnable() => button.onClick.AddListener(OnButtonClick);

        private void OnDisable() => button.onClick.RemoveListener(OnButtonClick);

        private void OnValidate() => button ??= GetComponent<Button>();

        protected virtual void OnButtonClick() => _command.Execute(argument1, argument2, argument3, argument4, argument5);
    }
}