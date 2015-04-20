/// ===========================================================================================
/// This currently serves as both the blog admin, user admin, and content admin Javascript area
/// ===========================================================================================

blog_class = function() {
    // Track whether the blog has been saved or not on the add Blog page
    this.blogIsSaved = false;
    this.blogId = -1;
    this.blogSpaceReplacementChar = '-';
};

blog_class.prototype.initPageEvents = function() {
    this.editBlogEvents();
    this.addBlogEvents();
    this.initPermaLinkEvents();
    this.manageBlogEvents();
    //this.initTinyMCEEvents();
    this.initPreviewEvent();
    this.initPublishDateEvents();
    
    if ($('.editBlog').length > 0) {
        this.initTagEvents();
    }

    
    // Allow sorting on Blog Admin Modules
    if ($("#Sortable1").length > 0) {
        this.initBlogAdminModuleEvents();
    }

    // Ckeditor instances
    if ($('#CKEDITBLOG').length > 0) {
        this.CKPageEditor = CKEDITOR.replace('CKEDITBLOG', {
            // options here
            height: 412
        });
    }

    if ($('#ShortDescription').length > 0) {
        this.CKShortDescEditor = CKEDITOR.replace('ShortDescription', {
            toolbar: [['Source', '-', 'Bold', 'Italic']],
            removePlugins: 'insertimage'
        });
    }
};

blog_class.prototype.initBlogAdminModuleEvents = function () {
    var self = this;

    // Sortable widgets
    $("#Sortable1, #Sortable2").sortable({
        placeholder: "ui-state-highlight",
        connectWith: ".connectedSortable",
        items: ".sortable", // Don't Sort editor
        handle: "h4",
        helper: 'clone', // Prevents click event on h4 after drag
        stop: function(event, ui) {
            // fire off save event after drop is complete
            self.saveModulePositions();
        }
    });

    // Collapse widgets
    $("div.editBlog div.connectedSortable div.panel h4").click(function() {
        $(this).parent().toggleClass("collapsed");

        // fire off save event
        self.saveModulePositions();
    });
};

blog_class.prototype.saveModulePositions = function() {
    var self = this;

    // Cancel any previos ajax requests
    if (self.xhr != null) {
        self.xhr.abort();
    }
    var AdminModulesColumn1 = [];
    var AdminModulesColumn2 = [];

    var count = 0;
    $("#Sortable1 div.panel.sortable").each(function () {

        AdminModulesColumn1.push({
            ModuleName : $(this).attr("id"),
            IsCollapsed: $(this).hasClass("collapsed"),
            OrderNumber: count,
            ColumnNumber : 1
        });
        
        count++;
    });

    var count2 = 0;
    $("#Sortable2 div.panel.sortable").each(function() {

        AdminModulesColumn2.push({
            ModuleName : $(this).attr("id"),
            IsCollapsed: $(this).hasClass("collapsed"),
            OrderNumber: count2,
            ColumnNumber: 2
        });
        
        count2++;
    });

    var data = {
        entity: {
            AdminModulesColumn1: AdminModulesColumn1,
            AdminModulesColumn2: AdminModulesColumn2
        }
    };
    self.xhr = $.ajax({
        url: "/admin/blog/savemodules/",
        type: "POST",
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(data, null, 2),
        success: function (data) {
            // Nothing to do yet
        }
    });

};

blog_class.prototype.initPermaLinkEvents = function () {
    // Edit Permalink on edit blog page
    var self = this;

    $("#EditPermaLink, #PermaLinkEnd").click(function () {
        var textVal = $("#PermaLinkEnd").text();
        $("#PermaLinkEditPane").removeClass("hide").show().attr("value", textVal);
        $(this).hide();
        $("#PermaLinkEnd").hide();

        $("#PermaLinkEditPane").focus();
    });

    // Key up format the permalink
    $("#PermaLinkEditPane").keyup(function () {
        $(this).attr("value", self.formatBlogLink($(this).val()));
    });

    // Hide the edit pane and update permalink
    $("#PermaLinkEditPane").blur(function () {

        // Clean up permalink by removing spaces and replacing with dashes
        // Remove all illegal characters
        var val = self.formatBlogLink($(this).val());

        $(this).attr("value", val);

        // Set the text with the new value
        $("#PermaLinkEnd").text($(this).attr("value"));
        $(this).hide();
        $("#EditPermaLink , #PermaLinkEnd").show();

        // Let the editor know we've made a modification
        $("#PermaLinkEnd").attr("data-modified", "true");
    });
};


// Replace spaces with appropriate character (such as dash)
// Remove illegal characters
blog_class.prototype.formatBlogLink = function (value) {
    // replace spaces with whatever (currently dashes)
    value = value.replace(/ /g, this.blogSpaceReplacementChar);

    // Strip bad characters
    return value.replace(/[^a-zA-Z0-9-_ ]/g, '');
};

blog_class.prototype.initPublishDateEvents = function() {
    $("#PublishDate").datepicker();
};

blog_class.prototype.initPreviewEvent = function() {
    $("#PreviewBlogButton").click(function() {
        var title = $("#PermaLinkEnd").text().toLowerCase();
        var host = 'http://' + window.location.host + '/blog/' + $('.category-permalink').text();

        // Open the blog in a new window
        window.open(host + title + '/' + '?debug=true');
    });
};

blog_class.prototype.manageBlogEvents = function() {
    var self = this;

    // Delete blog pop up
    $("div.manageBlogs").on("click", "a.btn.delete", function() {
        self.manageBlogId = $(this).attr("data-id");
        self.$manageBlogRow = $(this).parent().parent();
        var title = '"' + self.$manageBlogRow.find("td.title a").text() + '"';
        $("#popTitle").text(title);
        $("#DeleteModal").modal();
    });

    // Confirm Delete Blog
    $(document).on("click", "#ConfirmBlogDelete", function() {
        var id = self.manageBlogId;
        var $container = $("#DeleteModal > div.content");
        common.showAjaxLoader($container);
        $.ajax({
            url: "/admin/blog/deleteblog",
            type: "POST",
            data: {
                id: self.manageBlogId
            },
            success: function(data) {
                noty({ text: 'Blog Successfully Deleted.', type: 'success', timeout: 2000 });
                self.$manageBlogRow.remove();
                common.hideAjaxLoader($container);
                $('#DeleteModal').modal('hide');
            },
            error: function (data) {
                common.hideAjaxLoader($container);
                $('#DeleteModal').modal('hide');
                noty({ text: 'There was an error processing your request.', type: 'error' });
            }
        });
    });
};

blog_class.prototype.editBlogEvents = function() {
    var self = this;

    // Set startup information
    if ($("div.editBlog").length > 0) {
        self.blogId = $("div.editBlog").attr("data-id");
        self.blogIsSaved = true;
    }
};

blog_class.prototype.addBlogEvents = function() {
    var self = this;

    // Set blog title permalink hint on keypress
    $("#BlogTitle").keyup(function () {
        
        // Only update permalink if it hasn't been touched yet
        var permLinkModified = $("#PermaLinkEnd").attr("data-modified").toLowerCase() == "true";
        if (!permLinkModified) {
            console.log('fie')
            var val = $(this).val();
            val = val.replace(/ /g, self.blogSpaceReplacementChar);
            $("#PermaLinkEnd").text(val);
        }
    });

    // Fix spacing issues on page load
    $("#BlogTitle").trigger("keyup");

    // set permalink prefix
    $("#CategoriesModule select").change(function () {
       
        var catName = $(this).val()
            .replace(/&+/g, "and")
            .replace(/[^a-zA-Z0-9 ]/g, "")
            .replace(/ /g, self.blogSpaceReplacementChar).toLowerCase();
        $('.category-permalink').text(encodeURI(catName) + '/');
    });

    // Save Blog from edit / add blog page
    $("#SaveBlog").click(function() {
        var content = CKEDITOR.instances.CKEDITBLOG.getData();
        var featText = CKEDITOR.instances.ShortDescription.getData();
        $("#PermaLinkEnd").attr("data-modified", "true")
        var data = {
            entity: {
                Title: $("#BlogTitle").val(),
                HtmlContent: content,
                ImageUrl: $("#FeaturedImage").val(),
                IsActive: $("#IsPublicCheck").is(":checked"),
                IsFeatured: $("#IsFeaturedCheck").is(":checked"),
                Author: $("#Author option:selected").text(),
                AuthorId: $("#Author option:selected").attr("data-id"),
                BlogID: self.blogId,
                Category: $("#CategoriesModule select option:selected").val(),
                Tags: $("#BlogTags").val(),
                ShortDesc: featText,
                MetaDescription: $("#MetaDescription").val(),
                OGTitle: $("#OGTitle").val(),
                OGImage: $("#OGImage").val(),
                OGType: $("#OGType").val(),
                OGUrl: $("#OGUrl").val(),
                Canonical: $("#Canonical").val(),
                Date: $("#PublishDate").val(),
                PermaLink: self.formatBlogLink($("#PermaLinkEnd").text())
            }
        };

        // Currently all we really need is a title for validation
        if ($("#BlogTitle").attr("value").length < 1) {
            alert("Please enter a title.");
            return false;
        }
        
        $("#SaveSpinner").show();
        var postUrl = self.blogIsSaved ? "/admin/blog/modifyblog" : "/admin/blog/addblog";
        
        $.ajax({
            url: postUrl,
            type: "POST",
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data, null, 2),
            success: function(data) {
                self.blogIsSaved = true;
                self.blogId = data.id;

                var noty_id = noty({ text: 'Changes saved successfully.', type: 'success', timeout: 3000 });
                $("#SaveSpinner").hide();
            },
            error: function(data) {
                var noty_id = noty({ text: 'There was an error processing your request.', type: 'error' });
                $("#SaveSpinner").hide();
            }
        });
    });
};

blog_class.prototype.initTinyMCEEvents = function() {
    //tinymce on blog editor
    $('div.editArea textarea').tinymce({
        // Location of TinyMCE script
        script_url: '/Scripts/tinyMCE/tiny_mce.js',
        height: "450px",
        // General options
        theme: "advanced",
        skin: "o2k7",
        skin_variant: "silver",
        plugins: "autolink,spellchecker,lists,save,advimage,advlink,iespell,inlinepopups,insertdatetime,media,searchreplace,contextmenu,directionality,noneditable,template,safari,xhtmlxtras",

        // Theme options
        theme_advanced_buttons1: "bold,italic,underline,strikethrough,|,justifyleft,justifycenter,justifyright,justifyfull,|,bullist,numlist,|,outdent,indent,|,forecolor,backcolor,|,fontselect,fontsizeselect,link,|,code,|,blockquote,|,image,|,insertdate,inserttime,|,sub,sup,spellchecker",
        theme_advanced_buttons2: "",
        theme_advanced_buttons3: "",

        theme_advanced_toolbar_location: "top",
        theme_advanced_toolbar_align: "left",
        theme_advanced_statusbar_location: "bottom",
        theme_advanced_resizing: false,

        // Example content CSS (should be your site CSS)
        content_css: "/CSS/site.css",

        // Drop lists for link/image/media/template dialogs
        template_external_list_url: "lists/template_list.js",
        external_link_list_url: "lists/link_list.js",
        external_image_list_url: "lists/image_list.js",
        media_external_list_url: "lists/media_list.js",
    });

};

blog_class.prototype.initTagEvents = function () {

    // Tag Manager
    $("#BlogTags").tagit({
        allowSpaces: true
    });

    $('.commonTags a').click(function () {
        var tag = $(this).attr("data-tag");

        $('#BlogTags').tagit("createTag", tag);

        return false;
    });
};

// Keep at the bottom
$(document).ready(function() {
    blog = new blog_class();
    blog.initPageEvents();
});