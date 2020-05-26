var $url = '/updateSystem';

var data = utils.init({
  isNightly: null,
  version: null,
  
  pageIndex: 1,
  newCms: {},
  isShouldUpdate: false,
  updatesUrl: '',
  errorMessage: null
});

var methods = {
  load: function() {
    var $this = this;

    $api.get($url).then(function (response) {
      var res = response.data;

      $this.isNightly = res.isNightly;
      $this.version = res.version;

      utils.loading($this, false);

      $this.getVersion();
    }).catch(function (error) {
      utils.loading($this, false);
      utils.error(error);
    });
  },

  getVersion: function () {
    var $this = this;
    
    $cloud.getReleases(this.isNightly, this.version, null, function (cms) {
      $this.newCms = cms;
      $this.newCms.current = $this.version;
      $this.isShouldUpdate = utils.compareVersion($this.version, $this.newCms.version) == -1;
      utils.loading($this, false);
    });

    // ssApi.get({
    //   isNightly: isNightly,
    //   version: version
    // }, function (err, res) {
    //   if (err || !res || !res.value) return;

    //   $this.newCms = res.value;
    //   $this.isShouldUpdate = utils.compareVersion($this.version, $this.newCms.version) == -1;
    //   var major = $this.newCms.version.split('.')[0];
    //   var minor = $this.newCms.version.split('.')[1];
    //   $this.updatesUrl = 'https://www.siteserver.cn/updates/v' + major + '_' + minor + '/index.html';
    // }, 'newCmss', newCmsId);
  },

  updateSsCms: function () {
    if (!this.newCms) return;
    this.pageIndex = 2;
    var $this = this;

    $api.post($url, {
      version: $this.newCms.version
    }, function (err, res) {
      if (err) {
        $this.errorMessage = err.message;
      } else if (res) {
        $this.pageIndex = 3;
      }
    });
  },

  btnEnterClick: function() {
    location.href = '../';
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.load();
  }
});
