/*
*
* jmNotify.js
* DEVELOPED BY
* MORENO DI DOMENICO
* www.moretech.it
* moreno@moretech.it
* 05 Aug 2011
*
*
* http://www.moretech.it/demo/plugins/jmNotify
*
*/

(function($){

  // declare plugin name
  $.fn.jmNotify = function( options ) {
  
  		// declare default options
        var defaults = {
			
			/** options **/
			'methodIn'  	: 'fadeIn',
			'methodOut' 	: 'fadeOut',
			'speedIn'   	: 'normal',
			'speedOut'  	: 'normal',
			'delay'	    	: 3000,
			'html'      	: '',
			'closeButton'	: 'jm-close',
			'closeText'		: '',
			
			/** methods **/	
			'onStart'    	: '',
			'onComplete' 	: ''
		}
		
		options = $.extend(defaults, options);
		
		var $this;

		 // prevent more clicks when single notify is open
		 preventClicks = function()
		 {
			 $this.is(':visible') ? valid = false : valid = true;
			 return valid;
		 }
		 
		 
		 // close Notify
		 closeNotify = function()
		 {
			 if( $this.find('.'+options.closeButton).length == 0)
			 $this.append('<a href="#" class="'+options.closeButton+'">'+options.closeText+'</a>');
			 
			 
			 $('.'+options.closeButton).bind("click", function(){
				$this.hide();
				return false;
			 });
		 }
		
		 
		 // callback onStart
		 onStart = function()
		 {
			if(options.onStart)
			options.onStart();
		 }
		
		 // callback onComplete			 
		 onComplete = function()
		 {
			if(options.onComplete)
			options.onComplete();
		 }
		
		
		 // set new html
		 setHtml = function()
		 {
			 if(options.html)
			 {
				$this.html(options.html); 
			 }
		 }
		
		 // hide notify			 
		 hideNotify = function()
		 {
			// switch methodOut
			 switch (options.methodOut)
			 {
				case 'fadeOut':
					$this.delay(options.delay).fadeOut(options.speedOut, function(){ onComplete(); });
				break;
		
				case 'slideUp':
					$this.delay(options.delay).slideUp(options.speedOut, function(){ onComplete(); });
				break;

				default:
					$this.delay(options.delay).toggle(1, function(){ onComplete(); });
				break;
				
			 }
		 }
		 
		 // show notify
		 showNotify = function()
		 {
			 // switch methodIn
			 switch (options.methodIn)
			 {
				case 'fadeIn':
					$this.fadeIn( options.speedIn, function(){ hideNotify()} );
				break;

				case 'slideDown':
					$this.slideDown( options.speedIn, function(){ hideNotify()} );
				break;

				default:
					$this.toggle(1, function(){ hideNotify();});
				break;
			 }
		 }
		 
		 // init plugin
		 init = function()
		 {
			 if(preventClicks())
			 {
				 closeNotify();
				 onStart();
				 setHtml();
				 showNotify();
			 }
		 }
		 
	
		
		// cycling all elements
		return this.each(function(){
			 $this = $(this);
			 init();
		});

  };
})(jQuery);