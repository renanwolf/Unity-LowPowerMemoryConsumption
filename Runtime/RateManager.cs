using System;
using System.Collections.Generic;
using UnityEngine;
using UniRate.Debug;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#endif

using UnityObject = UnityEngine.Object;

namespace UniRate {

    [DisallowMultipleComponent]
    public sealed class RateManager : MonoBehaviour {

        #region <<---------- Initializers ---------->>

        private RateManager() { }

        #endregion <<---------- Initializers ---------->>




        #region <<---------- Instance ---------->>

        /// <summary>
        /// The singleton instance of <see cref="RateManager"/>.
        /// </summary>
        public static RateManager Instance {
            get {
                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    return _instance;
                }
                #endif
                if (!IsApplicationQuitting && _instance == null) {
                    _instance = UnityObject.FindObjectOfType<RateManager>();
                    if (_instance == null) {
                        var go = new GameObject();
                        _instance = go.AddComponent<RateManager>();
                    }
                }
                return _instance;
            }
        }
        private static RateManager _instance;

        /// <summary>
        /// Check if the instance exists without creating it.
        /// </summary>
        public static bool HasInstance => (_instance != null);

        #endregion <<---------- Instance ---------->>




        #region <<---------- UpdateRate Properties and Fields ---------->>

        [SerializeField] private UpdateRateMode _updateRateMode = UpdateRateMode.VSyncCount;
        [SerializeField] private int _minimumUpdateRate = 15;

        /// <summary>
        /// Update rate mode.
        /// </summary>
        public UpdateRateMode UpdateRateMode {
            get => this._updateRateMode;
            set {
                if (this._updateRateMode == value) return;
                this._updateRateMode = value;
                this.OnUpdateRateModeChanged(this._updateRateMode);
            }
        }

        /// <summary>
        /// Minimum update rate per seconds allowed.
        /// </summary>
        public int MinimumUpdateRate {
            get => this._minimumUpdateRate;
            set {
                if (this._minimumUpdateRate == value) return;
                this._minimumUpdateRate = value;
                this.OnMinimumUpdateRateChanged(this._minimumUpdateRate);
            }
        }

        /// <summary>
        /// Current update rate per seconds.
        /// </summary>
        public int UpdateRate {
            get => this._updateRate;
            private set {
                if (this._updateRate == value) return;
                this._updateRate = value;
                this.OnUpdateRateChanged(this._updateRate);
            }
        }
        private int _updateRate;

        /// <summary>
        /// Target update rate per seconds.
        /// </summary>
        public int TargetUpdateRate {
            get => this.ApplyTargetUpdateRateIfDirty();
            private set {
                if (this._targetUpdateRate == value) return;
                this._targetUpdateRate = value;
                this.OnTargetUpdateRateChanged(this._targetUpdateRate);
            }
        }
        private int _targetUpdateRate;

        /// <summary>
		/// Event raised when <see cref="UpdateRateMode"/> changes.
		/// </summary>
        public event Action<RateManager, UpdateRateMode> UpdateRateModeChanged {
            add {
                this._updateRateModeChanged -= value;
                this._updateRateModeChanged += value;
            }
            remove {
                this._updateRateModeChanged -= value;
            }
        }
        private Action<RateManager, UpdateRateMode> _updateRateModeChanged;

        /// <summary>
		/// Event raised when <see cref="UpdateRate"/> changes.
		/// </summary>
        public event Action<RateManager, int> UpdateRateChanged {
            add {
                this._updateRateChanged -= value;
                this._updateRateChanged += value;
            }
            remove {
                this._updateRateChanged -= value;
            }
        }
        private Action<RateManager, int> _updateRateChanged;

        /// <summary>
		/// Event raised when <see cref="TargetUpdateRate"/> changes.
		/// </summary>
        public event Action<RateManager, int> TargetUpdateRateChanged {
            add {
                this._targetUpdateRateChanged -= value;
                this._targetUpdateRateChanged += value;
            }
            remove {
                this._targetUpdateRateChanged -= value;
            }
        }
        private Action<RateManager, int> _targetUpdateRateChanged;

        private bool _targetUpdateRateDirty = true;
        private readonly HashSet<UpdateRateRequest> _updateRateRequests = new HashSet<UpdateRateRequest>();

        #endregion <<---------- UpdateRate Properties and Fields ---------->>




        #region <<---------- FixedUpdateRate Properties and Fields ---------->>

        [Space]
        [SerializeField] private int _minimumFixedUpdateRate = 15;

        /// <summary>
        /// Minimum fixed update rate per seconds allowed.
        /// </summary>
        public int MinimumFixedUpdateRate {
            get => this._minimumFixedUpdateRate;
            set {
                if (this._minimumFixedUpdateRate == value) return;
                this._minimumFixedUpdateRate = value;
                this.OnMinimumFixedUpdateRateChanged(this._minimumFixedUpdateRate);
            }
        }

        /// <summary>
        /// Current fixed update rate per seconds.
        /// </summary>
        public int FixedUpdateRate {
            get => this._fixedUpdateRate;
            private set {
                if (this._fixedUpdateRate == value) return;
                this._fixedUpdateRate = value;
                this.OnFixedUpdateRateChanged(this._fixedUpdateRate);
            }
        }
        private int _fixedUpdateRate;

        /// <summary>
        /// Target fixed update rate per seconds.
        /// </summary>
        public int TargetFixedUpdateRate {
            get => this.ApplyTargetFixedUpdateRateIfDirty();
            private set {
                if (this._targetFixedUpdateRate == value) return;
                this._targetFixedUpdateRate = value;
                this.OnTargetFixedUpdateRateChanged(this._targetFixedUpdateRate);
            }
        }
        private int _targetFixedUpdateRate;

        /// <summary>
		/// Event raised when <see cref="FixedUpdateRate"/> changes.
		/// </summary>
        public event Action<RateManager, int> FixedUpdateRateChanged {
            add {
                this._fixedUpdateRateChanged -= value;
                this._fixedUpdateRateChanged += value;
            }
            remove {
                this._fixedUpdateRateChanged -= value;
            }
        }
        private Action<RateManager, int> _fixedUpdateRateChanged;

        /// <summary>
		/// Event raised when <see cref="TargetFixedUpdateRate"/> changes.
		/// </summary>
        public event Action<RateManager, int> TargetFixedUpdateRateChanged {
            add {
                this._targetFixedUpdateRateChanged -= value;
                this._targetFixedUpdateRateChanged += value;
            }
            remove {
                this._targetFixedUpdateRateChanged -= value;
            }
        }
        private Action<RateManager, int> _targetFixedUpdateRateChanged;

        private bool _targetFixedUpdateRateDirty = true;
        private readonly HashSet<FixedUpdateRateRequest> _fixedUpdateRateRequests = new HashSet<FixedUpdateRateRequest>();

        #endregion <<---------- FixedUpdateRate Properties and Fields ---------->>




        #region <<---------- RenderInterval Properties and Fields ---------->>

        [Space]
        [SerializeField] private int _maximumRenderInterval = 3;

        /// <summary>
        /// Maximum render interval allowed.
        /// </summary>
        public int MaximumRenderInterval {
            get => this._maximumRenderInterval;
            set {
                if (this._maximumRenderInterval == value) return;
                this._maximumRenderInterval = value;
                this.OnMaximumRenderIntervalChanged(this._maximumRenderInterval);
            }
        }

        /// <summary>
        /// Current render interval.
        /// </summary>
        public int RenderInterval {
            get => this._renderInterval;
            private set {
                if (this._renderInterval == value) return;
                this._renderInterval = value;
                this.OnRenderIntervalChanged(this._renderInterval);
            }
        }
        private int _renderInterval;

        /// <summary>
        /// Target render interval.
        /// </summary>
        public int TargetRenderInterval {
            get => this.ApplyTargetRenderIntervalIfDirty();
            private set {
                if (this._targetRenderInterval == value) return;
                this._targetRenderInterval = value;
                this.OnTargetRenderIntervalChanged(this._targetRenderInterval);
            }
        }
        private int _targetRenderInterval;

        /// <summary>
		/// Event raised when <see cref="RenderInterval"/> changes.
		/// </summary>
        public event Action<RateManager, int> RenderIntervalChanged {
            add {
                this._renderIntervalChanged -= value;
                this._renderIntervalChanged += value;
            }
            remove {
                this._renderIntervalChanged -= value;
            }
        }
        private Action<RateManager, int> _renderIntervalChanged;

        /// <summary>
		/// Event raised when <see cref="TargetRenderInterval"/> changes.
		/// </summary>
        public event Action<RateManager, int> TargetRenderIntervalChanged {
            add {
                this._targetRenderIntervalChanged -= value;
                this._targetRenderIntervalChanged += value;
            }
            remove {
                this._targetRenderIntervalChanged -= value;
            }
        }
        private Action<RateManager, int> _targetRenderIntervalChanged;

        /// <summary>
        /// Current render rate.
        /// </summary>
        public int RenderRate {
            get {
                #if !UNITY_2019_3_OR_NEWER
                return this._updateRate;
                #else
                return (this._updateRate / this._renderInterval);
                #endif
            }
        }

        /// <summary>
        /// Will current frame render?
        /// </summary>
        public bool WillRender {
            get {
                #if !UNITY_2019_3_OR_NEWER
                return true;
                #else
                return OnDemandRendering.willCurrentFrameRender;
                #endif
            }
        }

        /// <summary>
        /// Is render interval supported?
        /// </summary>
        /// <value></value>
        public bool IsRenderIntervalSupported {
            get {
                #if UNITY_2019_3_OR_NEWER
                return true;
                #else
                return false;
                #endif
            }
        }

        private bool _targetRenderIntervalDirty = true;
        private readonly HashSet<RenderIntervalRequest> _renderIntervalRequests = new HashSet<RenderIntervalRequest>();
        
        #if !UNITY_2019_3_OR_NEWER
        private bool _loggedRenderIntervalNotSupported;
        #endif
        
        #endregion <<---------- RenderInterval Properties and Fields ---------->>




        #region <<---------- Properties and Fields ---------->>

        internal static bool IsApplicationQuitting { get; private set; }

        private static readonly string GameObjectName = $"UniRate ({nameof(RateManager)})";

        private readonly GUIContent _guiContentText = new GUIContent();

        #endregion <<---------- Properties and Fields ---------->>




        #region <<---------- MonoBehaviour ---------->>

        private void Awake() {

            if (_instance == null) {
                _instance = this;
            }
            else if (_instance != this) {
                if (RateDebug.IsLogLevelActive(RateLogLevel.Warning)) {
                    RateDebug.Log(RateLogLevel.Warning, $"trying to awake another instance of {nameof(RateManager)} at '{this.gameObject.scene.name}/{this.gameObject.name}', destroying it", this.gameObject);
                }
                Destroy(this);
                return;
            }

            this.gameObject.name = GameObjectName;
            this.transform.SetParent(null, false);
            DontDestroyOnLoad(this);
        }

        private void OnEnable() {
            this._targetUpdateRateDirty = true;
            this._targetFixedUpdateRateDirty = true;
            this._targetRenderIntervalDirty = true;
        }

        private void Update() {
            
            this.ApplyRenderIntervalUnitySettings(this.TargetRenderInterval);
            #if UNITY_2019_3_OR_NEWER
            this.RenderInterval = OnDemandRendering.renderFrameInterval;
            #else
            this.RenderInterval = 1;
            #endif

            this.ApplyUpdateRateUnitySettings(this._updateRateMode, this.TargetUpdateRate);
            this.UpdateRate = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        }

        private void FixedUpdate() {
            this.ApplyFixedUpdateRateUnitySettings(this.TargetFixedUpdateRate);
            this.FixedUpdateRate = Mathf.RoundToInt(1f / Time.fixedUnscaledDeltaTime);
        }

        private void OnApplicationQuit() {
            IsApplicationQuitting = true;
        }

        private void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }

        #if UNITY_EDITOR

        private void OnValidate() {
            if (!Application.isPlaying) return;
            this._targetUpdateRateDirty = true;
            this._targetFixedUpdateRateDirty = true;
            this._targetRenderIntervalDirty = true;
            this.ApplyUpdateRateUnitySettings(this._updateRateMode, this.TargetUpdateRate);
        }

        private void Reset() {
            this.gameObject.name = GameObjectName;
        }

        #endif

        private void OnGUI() {
            if (!RateDebug.DisplayOnScreenData) return;

            if (RateDebug.ScreenDataVerbose) {
                this._guiContentText.text = $"UpdateMode: {this._updateRateMode.ToString()}\nUpdate/s: {this._updateRate.ToString("000")} / {this._targetUpdateRate.ToString("000")}\nFixedUpdate/s: {this._fixedUpdateRate.ToString("000")} / {this._targetFixedUpdateRate.ToString("000")}\n{(this.IsRenderIntervalSupported ? $"Will Render: {this.WillRender.ToString()}" : "Render interval not supported")}\nRenderInterval: {this._renderInterval.ToString()} / {this._targetRenderInterval.ToString()}\nRender/s: {this.RenderRate.ToString("000")}";
            }
            else {
                this._guiContentText.text = $"Update/s: {this._updateRate.ToString("000")} / {this._targetUpdateRate.ToString("000")}\nFixedUpdate/s: {this._fixedUpdateRate.ToString("000")} / {this._targetFixedUpdateRate.ToString("000")}\nRenderInterval: {this._renderInterval.ToString()} / {this._targetRenderInterval.ToString()}\nRender/s: {this.RenderRate.ToString("000")}";
            }

            var safeArea = Screen.safeArea;
            RateDebug.ScreenDataStyle.CalcMinMaxWidth(this._guiContentText, out float minWidth, out float maxWidth);
            var labelWidth = Mathf.Min(safeArea.width - 10, maxWidth);
            float labelSpaceH = Mathf.Min(5f, ((safeArea.width - labelWidth) / 2f));
            float labelSpaceV = Math.Max(Screen.height - safeArea.height, 5f);
            var rectLine = new Rect(safeArea.x + labelSpaceH, labelSpaceV, labelWidth, RateDebug.ScreenDataStyle.CalcHeight(this._guiContentText, labelWidth));

            if (GUI.Button(rectLine, this._guiContentText, RateDebug.ScreenDataStyle)) {
                RateDebug.ScreenDataVerbose = !RateDebug.ScreenDataVerbose;
            }
        }

        #endregion <<---------- MonoBehaviour ---------->>




        #region <<---------- UpdateRate Callbacks ---------->>

        private void OnUpdateRateModeChanged(UpdateRateMode updateRateMode) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Info)) {
                RateDebug.Log(RateLogLevel.Info, $"{nameof(this.UpdateRateMode)} changed to {updateRateMode.ToString()}");
            }
            this.ApplyUpdateRateUnitySettings(updateRateMode, this.TargetUpdateRate);
            var e = this._updateRateModeChanged;
            if (e == null) return;
            e(this, updateRateMode);
        }

        private void OnMinimumUpdateRateChanged(int minimumUpdateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Info)) {
                RateDebug.Log(RateLogLevel.Info, $"{nameof(this.MinimumUpdateRate)} changed to {minimumUpdateRate.ToString()}");
            }
            this._targetUpdateRateDirty = true;
        }

        private void OnUpdateRateChanged(int updateRate) {
            var e = this._updateRateChanged;
            if (e == null) return;
            e(this, updateRate);
        }

        private void OnTargetUpdateRateChanged(int targetUpdateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Debug)) {
                RateDebug.Log(RateLogLevel.Debug, $"{nameof(this.TargetUpdateRate)} changed to {targetUpdateRate.ToString()}");
            }
            this.ApplyUpdateRateUnitySettings(this._updateRateMode, targetUpdateRate);
            var e = this._targetUpdateRateChanged;
            if (e == null) return;
            e(this, targetUpdateRate);
        }

        private void OnUpdateRateRequestsChanged(IEnumerable<UpdateRateRequest> requests) {
            this._targetUpdateRateDirty = true;
        }

        #endregion <<---------- UpdateRate Callbacks ---------->>




        #region <<---------- FixedUpdateRate Callbacks ---------->>

        private void OnMinimumFixedUpdateRateChanged(int minimumFixedUpdateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Info)) {
                RateDebug.Log(RateLogLevel.Info, $"{nameof(this.MinimumFixedUpdateRate)} changed to {minimumFixedUpdateRate.ToString()}");
            }
            this._targetFixedUpdateRateDirty = true;
        }

        private void OnFixedUpdateRateChanged(int fixedUpdateRate) {
            var e = this._fixedUpdateRateChanged;
            if (e == null) return;
            e(this, fixedUpdateRate);
        }

        private void OnTargetFixedUpdateRateChanged(int targetFixedUpdateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Debug)) {
                RateDebug.Log(RateLogLevel.Debug, $"{nameof(this.TargetFixedUpdateRate)} changed to {targetFixedUpdateRate.ToString()}");
            }
            this.ApplyFixedUpdateRateUnitySettings(targetFixedUpdateRate);
            var e = this._targetFixedUpdateRateChanged;
            if (e == null) return;
            e(this, targetFixedUpdateRate);
        }

        private void OnFixedUpdateRateRequestsChanged(IEnumerable<FixedUpdateRateRequest> requests) {
            this._targetFixedUpdateRateDirty = true;
        }

        #endregion <<---------- FixedUpdateRate Callbacks ---------->>




        #region <<---------- RenderInterval Callbacks ---------->>
        
        private void OnMaximumRenderIntervalChanged(int maximumRenderInterval) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Info)) {
                RateDebug.Log(RateLogLevel.Info, $"{nameof(this.MaximumRenderInterval)} changed to {maximumRenderInterval.ToString()}");
            }
            this._targetRenderIntervalDirty = true;
        }

        private void OnRenderIntervalChanged(int renderInterval) {
            var e = this._renderIntervalChanged;
            if (e == null) return;
            e(this, renderInterval);
        }

        private void OnTargetRenderIntervalChanged(int targetRenderInterval) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Debug)) {
                RateDebug.Log(RateLogLevel.Debug, $"{nameof(this.TargetRenderInterval)} changed to {targetRenderInterval.ToString()}");
            }
            this.ApplyRenderIntervalUnitySettings(targetRenderInterval);
            var e = this._targetRenderIntervalChanged;
            if (e == null) return;
            e(this, targetRenderInterval);
        }

        private void OnRenderIntervalRequestsChanged(IEnumerable<RenderIntervalRequest> requests) {
            this._targetRenderIntervalDirty = true;
        }
        
        #endregion <<---------- RenderInterval Callbacks ---------->>




        #region <<---------- Requests ---------->>
        
        /// <summary>
        /// Create a new <see cref="UpdateRateRequest"/>.
        /// </summary>
        public UpdateRateRequest RequestUpdateRate(int updateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"creating update rate request {updateRate.ToString()}");
            }
            var request = new UpdateRateRequest(this, updateRate);
            this._updateRateRequests.Add(request);
            this.OnUpdateRateRequestsChanged(this._updateRateRequests);
            return request;
        }

        /// <summary>
        /// Create a new <see cref="UpdateRateRequest"/>.
        /// </summary>
        public UpdateRateRequest RequestUpdateRate(RatePreset preset) {
            return this.RequestUpdateRate(preset.UpdateRate);
        }

        /// <summary>
        /// Create a new <see cref="FixedUpdateRateRequest"/>.
        /// </summary>
        public FixedUpdateRateRequest RequestFixedUpdateRate(int fixedUpdateRate) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"creating fixed update rate request {fixedUpdateRate.ToString()}");
            }
            var request = new FixedUpdateRateRequest(this, fixedUpdateRate);
            this._fixedUpdateRateRequests.Add(request);
            this.OnFixedUpdateRateRequestsChanged(this._fixedUpdateRateRequests);
            return request;
        }

        /// <summary>
        /// Create a new <see cref="FixedUpdateRateRequest"/>.
        /// </summary>
        public FixedUpdateRateRequest RequestFixedUpdateRate(RatePreset preset) {
            return this.RequestFixedUpdateRate(preset.FixedUpdateRate);
        }

        /// <summary>
        /// Create a new <see cref="FixedUpdateRateRequest"/>.
        /// </summary>
        public RenderIntervalRequest RequestRenderInterval(int renderInterval) {
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"creating render interval request {renderInterval.ToString()}");
            }

            #if !UNITY_2019_3_OR_NEWER
            this.LogRenderIntervalNotSupportedOnce();
            #endif

            var request = new RenderIntervalRequest(this, renderInterval);
            this._renderIntervalRequests.Add(request);
            this.OnRenderIntervalRequestsChanged(this._renderIntervalRequests);
            return request;
        }

        /// <summary>
        /// Create a new <see cref="FixedUpdateRateRequest"/>.
        /// </summary>
        public RenderIntervalRequest RequestRenderInterval(RatePreset preset) {
            return this.RequestRenderInterval(preset.RenderInterval);
        }

        internal void CancelUpdateRateRequest(UpdateRateRequest request) {
            if (request == null) return;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"canceling update rate request {request.UpdateRate.ToString()}");
            }
            if (!this._updateRateRequests.Remove(request)) return;
            this.OnUpdateRateRequestsChanged(this._updateRateRequests);
        }

        internal void CancelFixedUpdateRateRequest(FixedUpdateRateRequest request) {
            if (request == null) return;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"canceling fixed update rate request {request.FixedUpdateRate.ToString()}");
            }
            if (!this._fixedUpdateRateRequests.Remove(request)) return;
            this.OnFixedUpdateRateRequestsChanged(this._fixedUpdateRateRequests);
        }

        internal void CancelRenderIntervalRequest(RenderIntervalRequest request) {
            if (request == null) return;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"canceling render interval request {request.RenderInterval.ToString()}");
            }

            #if !UNITY_2019_3_OR_NEWER
            this.LogRenderIntervalNotSupportedOnce();
            #endif
            
            if (!this._renderIntervalRequests.Remove(request)) return;
            this.OnRenderIntervalRequestsChanged(this._renderIntervalRequests);
        }
        
        #endregion <<---------- Requests ---------->>




        #region <<---------- General ---------->>

        private bool ApplyUpdateRateUnitySettings(UpdateRateMode updateRateMode, int targetUpdateRate) {
            int newVSyncCount = 1;
            int newTargetFrameRate = targetUpdateRate;

            switch (updateRateMode) {

                case UpdateRateMode.ApplicationTargetFrameRate:
                    newVSyncCount = 0;
                    newTargetFrameRate = targetUpdateRate;
                    break;

                case UpdateRateMode.VSyncCount:
                    // UNITY DOCS https://docs.unity3d.com/ScriptReference/QualitySettings-vSyncCount.html?_ga=2.244925125.1929096148.1586530815-1497395495.1586530815
                    // If QualitySettings.vSyncCount is set to a value other than 'Don't Sync' (0), the value of Application.targetFrameRate will be ignored.
                    // QualitySettings.vSyncCount value must be 0, 1, 2, 3, or 4.
                    // QualitySettings.vSyncCount is ignored on iOS.
                    newVSyncCount = Mathf.Clamp(
                        Screen.currentResolution.refreshRate / targetUpdateRate,
                        1,
                        4
                    );
                    newTargetFrameRate = targetUpdateRate;
                    break;

                default:
                    if (RateDebug.IsLogLevelActive(RateLogLevel.Error)) {
                        RateDebug.Log(RateLogLevel.Error, $"not handling {nameof(UpdateRateMode)}.{updateRateMode.ToString()}");
                    }
                    return false;
            }

            bool changed = false;

            if (QualitySettings.vSyncCount != newVSyncCount) {
                changed = true;
                if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                    RateDebug.Log(RateLogLevel.Trace, $"setting QualitySettings.vSyncCount to {newVSyncCount.ToString()}");
                }
                QualitySettings.vSyncCount = newVSyncCount;
            }

            if (Application.targetFrameRate != newTargetFrameRate) {
                changed = true;
                if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                    RateDebug.Log(RateLogLevel.Trace, $"setting Application.targetFrameRate to {newTargetFrameRate.ToString()}");
                }
                Application.targetFrameRate = newTargetFrameRate;
            }

            return changed;
        }

        private int CalculateTargetUpdateRate(IEnumerable<UpdateRateRequest> requests, int minimum) {
            var target = minimum;
            foreach (var request in requests) {
                if (request.IsDisposed || request.UpdateRate <= target) continue;
                target = request.UpdateRate;
            }
            return target;
        }

        private bool ApplyFixedUpdateRateUnitySettings(int targetFixedUpdateRate) {
            float newFixedDeltaTime = (1f / (float)targetFixedUpdateRate);
            if (newFixedDeltaTime == Time.fixedDeltaTime) return false;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"setting Time.fixedDeltaTime to {newFixedDeltaTime.ToString("0.0##")}");
            }
            Time.fixedDeltaTime = newFixedDeltaTime;
            return true;
        }

        private int CalculateTargetFixedUpdateRate(IEnumerable<FixedUpdateRateRequest> requests, int minimum) {
            var target = minimum;
            foreach (var request in requests) {
                if (request.IsDisposed || request.FixedUpdateRate <= target) continue;
                target = request.FixedUpdateRate;
            }
            return target;
        }

        #if !UNITY_2019_3_OR_NEWER

        private void LogRenderIntervalNotSupportedOnce() {
            if (this._loggedRenderIntervalNotSupported) return;
            this._loggedRenderIntervalNotSupported = true;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Warning)) {
                RateDebug.Log(RateLogLevel.Warning, $"render interval is only supported on Unity 2019.3 or newer");
            }
        }
        
        #endif

        private bool ApplyRenderIntervalUnitySettings(int targetRenderInterval) {
            #if UNITY_2019_3_OR_NEWER

            if (OnDemandRendering.renderFrameInterval == targetRenderInterval) return false;
            if (RateDebug.IsLogLevelActive(RateLogLevel.Trace)) {
                RateDebug.Log(RateLogLevel.Trace, $"setting OnDemandRendering.renderFrameInterval to {targetRenderInterval.ToString()}");
            }
            OnDemandRendering.renderFrameInterval = targetRenderInterval;
            return true;

            #else

            return false;

            #endif
        }

        private int CalculateTargetRenderInterval(IEnumerable<RenderIntervalRequest> requests, int maximum) {
            var target = maximum;
            foreach (var request in requests) {
                if (request.IsDisposed || request.RenderInterval >= target) continue;
                target = request.RenderInterval;
            }
            return target;
        }

        public void ApplyTargetsIfDirty() {
            this.ApplyTargetRenderIntervalIfDirty();
            this.ApplyTargetUpdateRateIfDirty();
            this.ApplyTargetFixedUpdateRateIfDirty();
        }

        private int ApplyTargetUpdateRateIfDirty() {
            if (!this._targetUpdateRateDirty) return this._targetUpdateRate;
            this._targetUpdateRateDirty = false;
            int target = this.CalculateTargetUpdateRate(this._updateRateRequests, this._minimumUpdateRate);
            this.TargetUpdateRate = target;
            return target;
        }

        private int ApplyTargetFixedUpdateRateIfDirty() {
            if (!this._targetFixedUpdateRateDirty) return this._targetFixedUpdateRate;
            this._targetFixedUpdateRateDirty = false;
            int target = this.CalculateTargetFixedUpdateRate(this._fixedUpdateRateRequests, this._minimumFixedUpdateRate);
            this.TargetFixedUpdateRate = target;
            return target;
        }

        private int ApplyTargetRenderIntervalIfDirty() {
            if (!this._targetRenderIntervalDirty) return this._targetRenderInterval;
            this._targetRenderIntervalDirty = false;
            int target = this.CalculateTargetRenderInterval(this._renderIntervalRequests, this._maximumRenderInterval);
            this.TargetRenderInterval = target;
            return target;
        }

        #endregion <<---------- General ---------->>
    }
}